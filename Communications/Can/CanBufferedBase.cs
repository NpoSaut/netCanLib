using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;

namespace Communications.Can
{
    public class CanBufferedBase
    {
        private ConcurrentQueue<CanFrame> FramesBuffer = new ConcurrentQueue<CanFrame>();

        protected void Enqueue(IEnumerable<CanFrame> Frames)
        {
            foreach (var f in Frames) Enqueue(f);
        }
        protected void Enqueue(CanFrame Frame)
        {
            FramesBuffer.Enqueue(Frame);
        }

        /// <summary>
        /// Выполняет блокирующее ожидание фрейма
        /// </summary>
        /// <returns>Найденный фрейм</returns>
        public CanFrame ReadFrame()
        {
            return ReadFrame(TimeSpan.FromMilliseconds(-1));
        }
        public CanFrame ReadFrame(TimeSpan Timeout)
        {
            CanFrame f = CanFrame.NewWithDescriptor(0);
            bool ok = false;
            SpinWait.SpinUntil(() => ok = FramesBuffer.TryDequeue(out f), Timeout);
            if (ok) return f;
            else throw new TimeoutException("Превышено время ожидания пакета");
        }
        /// <summary>
        /// Блокирующе считывает последовательность всех фреймов, приходящих в данный буфер
        /// </summary>
        /// <returns>Последовательность отловленных кадров</returns>
        /// <remarks>
        /// Поскольку функция блокирующая и считывает _все_ приходящие кадры, выход из этого перечисления не предусмотрен.
        /// Для правильной работы следует пользоваться лишь методами вроде методов, извлекающих N первых членов последовательности (First, Take, TakeWhile, ...)
        /// </remarks>
        public IEnumerable<CanFrame> Read()
        {
            while (true)
                yield return ReadFrame();
        }
        /// <summary>
        /// Блокирующе считывает последовательность всех фреймов, приходящих в данный буфер до тех пор, пока в течение указанного интервала приходит хоть одно сообщение
        /// </summary>
        /// <param name="Timeout">Время, по истечении которого чтение следующих пакетов прекратится</param>
        /// <param name="ThrowExceptionOnTimeout">Следует ли пробрасывать TimeoutException по истечении таймаута (если нет - то чтение пакетов просто прекратится)</param>
        /// <returns>Последовательность отловленных кадров</returns>
        /// <remarks>
        /// Можно использовать для отлова серий подряд идущих сообщений (когда другая серия отделяется по времени).
        /// Параметр ThrowExceptionOnTimeout следует использовать, если сообщение гарантированно должно поступить и его задержка является признаком ошибки
        /// </remarks>
        public IEnumerable<CanFrame> Read(TimeSpan Timeout, Boolean ThrowExceptionOnTimeout = false)
        {
            while (true)
            {
                CanFrame f;

                try
                {
                    f = ReadFrame(Timeout);
                }
                catch (TimeoutException)
                {
                    if (ThrowExceptionOnTimeout) throw;
                    else yield break;
                }

                yield return f;
            }
        }
    }
}
