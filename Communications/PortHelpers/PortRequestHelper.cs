using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Communications.Options;

namespace Communications.PortHelpers
{
    public static class PortRequestHelper
    {
        /// <summary>Делает запрос по указанному порту</summary>
        /// <typeparam name="TFrame">Тип сообщения</typeparam>
        /// <typeparam name="TOptions">Тип опций порта</typeparam>
        /// <param name="Port">Порт для запроса</param>
        /// <param name="RequestFrame">Запрос</param>
        /// <param name="Timeout">Таймаут запроса</param>
        /// <returns>Ответ</returns>
        public static TFrame Request<TFrame, TOptions>(this IPort<TFrame, TOptions> Port, TFrame RequestFrame, TimeSpan Timeout)
            where TOptions : PortOptions<TFrame> { return Port.Request(RequestFrame, flow => flow.Timeout(Timeout).First()); }

        /// <summary>Делает запрос по указанному порту</summary>
        /// <typeparam name="TFrame">Тип сообщения</typeparam>
        /// <typeparam name="TOptions">Тип опций порта</typeparam>
        /// <param name="Port">Порт для запроса</param>
        /// <param name="RequestFrame">Запрос</param>
        /// <returns>Ответ</returns>
        public static TFrame Request<TFrame, TOptions>(this IPort<TFrame, TOptions> Port, TFrame RequestFrame)
            where TOptions : PortOptions<TFrame> { return Port.Request(RequestFrame, flow => flow.First()); }

        /// <summary>Делает запрос по указанному порту</summary>
        /// <remarks>На вход <paramref name="ResultSelector" /> подаётся поток сообщений, полученных после отправки запроса</remarks>
        /// <typeparam name="TFrame">Тип сообщения</typeparam>
        /// <typeparam name="TResult">Тип возвращаемого результата</typeparam>
        /// <typeparam name="TOptions">Тип опций порта</typeparam>
        /// <param name="Port">Порт для запроса</param>
        /// <param name="RequestFrame">Запрос</param>
        /// <param name="ResultSelector">Селектор результата запроса</param>
        /// <returns>Ответ на запрос, обработанный селектором <paramref name="ResultSelector" /></returns>
        public static TResult Request<TFrame, TOptions, TResult>(this IPort<TFrame, TOptions> Port, TFrame RequestFrame,
                                                                 Func<IObservable<TFrame>, TResult> ResultSelector)
            where TOptions : PortOptions<TFrame>
        {
            IConnectableObservable<TFrame> flow = Port.Rx.WaitForTransactionCompleated().Replay();
            using (flow.Connect())
            {
                Port.BeginSend(RequestFrame).Wait();
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
