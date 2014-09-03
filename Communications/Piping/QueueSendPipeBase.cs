using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Communications.Piping
{
    /// <summary>Труба, реализующая очередь на отправку</summary>
    /// <typeparam name="TDatagram">Тип дейтаграмм</typeparam>
    public abstract class QueueSendPipeBase<TDatagram> : SendPipeBase<TDatagram>
    {
        private readonly object _takeLocker = new object();

        public QueueSendPipeBase()
        {
            ScheduledTasks = new ConcurrentQueue<SendTask>();
            ActiveTasks = new Queue<SendTask>();
        }

        /// <summary>Задачи, запланированные к отправке</summary>
        protected ConcurrentQueue<SendTask> ScheduledTasks { get; private set; }

        /// <summary>Задачи, отправка сообщений из которых осуществляется</summary>
        protected Queue<SendTask> ActiveTasks { get; private set; }

        /// <summary>Передаёт дейтаграммы на низлежащий уровень для его отправки</summary>
        /// <param name="Datagrams">Кадры для отправки</param>
        /// <param name="Timeout">Таймаут операции</param>
        public override void Send(IList<TDatagram> Datagrams, TimeSpan Timeout)
        {
            var task = new SendTask(Datagrams, Timeout);
            EnqueueNewTask(task);
            task.WaitForFinished();
        }

        /// <summary>Ставит задачу в очередь на обработку</summary>
        protected virtual void EnqueueNewTask(SendTask Task) { ScheduledTasks.Enqueue(Task); }

        /// <summary>Изымает из очереди на отправку не более указанного количества дейтаграмм</summary>
        /// <param name="Limit">Максимальное количество дейтаграмм, которые следует изъять из очереди</param>
        /// <returns>Список дейтаграмм, изъятых из очереди на отправку</returns>
        public virtual IList<TDatagram> Take(int Limit)
        {
            var res = new List<TDatagram>(Limit);

            // Блокируемся, чтобы избежать коллизий с одновременным разбором задач из разных потоков
            lock (_takeLocker)
            {
                // Набираем коллекцию, пока не достигнем лимита или не выберем всю очередь
                while (res.Count < Limit && !ScheduledTasks.IsEmpty)
                {
                    SendTask task;

                    // Берём первую стоящую в очереди задачу
                    ScheduledTasks.TryPeek(out task);

                    // Если задача актуальна, берём из неё нужное количество элементов
                    if (task.IsActual)
                    {
                        res.AddRange(task.Take(Limit - res.Count));
                        ActiveTasks.Enqueue(task);
                    }

                    // Если задача более не актуальна, или все её элементы взяты - убираем её из списка назначенных
                    if (task.IsTaken || !task.IsActual)
                        ScheduledTasks.TryDequeue(out task);
                }
            }

            return res;
        }

        /// <summary>Отмечает первые <paramref name="Count" /> дейтаграмм в очереди как отправленные</summary>
        /// <param name="Count">Количество отправленных дейтаграмм</param>
        public void SetAsCompleated(int Count)
        {
            int i = 0; // Счётчик установленных статусов
            // Блокируемся на объекте, чтобы исключить изменение очереди в процессе установки
            lock (_takeLocker)
            {
                // Устанавливаем статусы, пока не достигнем нужного количества
                while (i < Count)
                {
                    // Берём первую задачу из очереди
                    SendTask task = ActiveTasks.Peek();
                    // Устанавливаем количество отправленных сообщений
                    i += task.TrySetAsCompleated(Count);
                    // Проверяем, не оказалось ли так, что задача выполнена
                    if (task.IsCompleated)
                    {
                        // Если задача выполнена, убираем её из очереди активных задач
                        ActiveTasks.Dequeue();
                        // Указываем, что задача выполнена
                        task.OnCompleated();
                    }
                }
            }
        }

        /// <summary>Задачка на отправку</summary>
        protected class SendTask
        {
            /// <summary>Объект для блокировки для ожидания завершения задачи</summary>
            private readonly object _locker = new object();

            private int _compleatedPointer;
            private int _takingPointer;

            public SendTask(IList<TDatagram> Datagrams, TimeSpan Timeout)
            {
                this.Timeout = Timeout;
                this.Datagrams = Datagrams;
                CreationTime = DateTime.Now;
            }

            public IList<TDatagram> Datagrams { get; private set; }
            public DateTime CreationTime { get; private set; }
            public TimeSpan Timeout { get; private set; }

            /// <summary>Показывает, актуальна ли эта задача</summary>
            public bool IsActual
            {
                get { return DateTime.Now <= CreationTime + Timeout; }
            }

            /// <summary>Показывает, все ли сообщения из этой задачи были взяты в обработку</summary>
            public bool IsTaken
            {
                get { return _takingPointer == Datagrams.Count; }
            }

            /// <summary>Показывает, является ли задача выполненной (все сообщения из задачи были отправлены)</summary>
            public bool IsCompleated
            {
                get { return _compleatedPointer == Datagrams.Count; }
            }

            /// <summary>Следует вызвать, когда отправка задачи завершится</summary>
            public void OnCompleated() { Monitor.PulseAll(_locker); }

            /// <summary>Дожидается окончания отправки сообщений</summary>
            public void WaitForFinished() { Monitor.Wait(_locker, Timeout); /* TODO: Сделать аккуратно */ }

            // ReSharper disable once ReturnTypeCanBeEnumerable.Global
            /// <summary>Берёт из задачи не более <paramref name="Limit" /> сообщений на отправку</summary>
            public IList<TDatagram> Take(int Limit)
            {
                List<TDatagram> res = Datagrams.Skip(_takingPointer).Take(Limit).ToList();
                _takingPointer += res.Count;
                return res;
            }

            /// <summary>Устанавливает первые <paramref name="Count" /> сообщений как отправленные</summary>
            public int TrySetAsCompleated(int Count)
            {
                int res = Math.Min(Count, Datagrams.Count - _compleatedPointer);
                _compleatedPointer += res;
                return res;
            }
        }
    }
}
