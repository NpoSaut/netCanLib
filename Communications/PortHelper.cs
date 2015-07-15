using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Communications
{
    public static class PortHelper
    {
        /// <summary>Отправляет сообщение в указанный порт</summary>
        /// <typeparam name="TFrame">Тип сообщения</typeparam>
        /// <param name="Port">Порт для отправки</param>
        /// <param name="Frame">Сообщение для отправки</param>
        public static void Send<TFrame>(this IPort<TFrame> Port, TFrame Frame)
        {
            Port.Tx.OnNext(Frame);
        }

        /// <summary>Делает запрос по указанному порту</summary>
        /// <typeparam name="TFrame">Тип сообщения</typeparam>
        /// <param name="Port">Порт для запроса</param>
        /// <param name="RequestFrame">Запрос</param>
        /// <returns>Ответ</returns>
        public static TFrame Request<TFrame>(this IPort<TFrame> Port, TFrame RequestFrame)
        {
            return Port.Request(RequestFrame, flow => flow.First());
        }

        /// <summary>Делает запрос по указанному порту</summary>
        /// <typeparam name="TFrame">Тип сообщения</typeparam>
        /// <param name="Port">Порт для запроса</param>
        /// <param name="RequestFrame">Запрос</param>
        /// <param name="Timeout">Таймаут запроса</param>
        /// <returns>Ответ</returns>
        public static TFrame Request<TFrame>(this IPort<TFrame> Port, TFrame RequestFrame, TimeSpan Timeout)
        {
            return Port.Request(RequestFrame, flow => flow.Timeout(Timeout).First());
        }

        /// <summary>Делает запрос по указанному порту</summary>
        /// <remarks>На вход <paramref name="ResultSelector" /> подаётся поток сообщений, полученных после отправки запроса</remarks>
        /// <typeparam name="TFrame">Тип сообщения</typeparam>
        /// <typeparam name="TResult">Тип возвращаемого результата</typeparam>
        /// <param name="Port">Порт для запроса</param>
        /// <param name="RequestFrame">Запрос</param>
        /// <param name="ResultSelector">Селектор результата запроса</param>
        /// <returns>Ответ</returns>
        public static TResult Request<TFrame, TResult>(this IPort<TFrame> Port, TFrame RequestFrame, Func<IObservable<TFrame>, TResult> ResultSelector)
        {
            IConnectableObservable<TFrame> flow = Port.Rx.Replay();
            using (flow.Connect())
            {
                Port.Send(RequestFrame);
                IObservable<TFrame> flowAfterRequest;

                if (Port.Options.ProducesLoopback)
                {
                    // Если портом поддерживаются Loopback-пакеты, пропускаем всё, что успели принять до запроса
                    ILoopbackInspector<TFrame> loopbackInspector = Port.Options.LoopbackInspector;
                    flowAfterRequest = flow.SkipWhile(f => !loopbackInspector.IsLoopback(f, RequestFrame))
                                           .Skip(1);
                }
                else
                    flowAfterRequest = flow;

                return ResultSelector(flowAfterRequest);
            }
        }
    }
}
