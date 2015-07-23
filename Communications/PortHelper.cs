using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Communications
{
    public static class PortHelper
    {
        /// <summary>Отправляет сообщение в указанный порт</summary>
        /// <typeparam name="TFrame">Тип сообщения</typeparam>
        /// <typeparam name="TOptions">Тип опций порта</typeparam>
        /// <param name="Port">Порт для отправки</param>
        /// <param name="Frame">Сообщение для отправки</param>
        public static void Send<TFrame, TOptions>(this IPort<TFrame, TOptions> Port, TFrame Frame)
            where TOptions : PortOptions<TFrame>
        {
            Port.Tx.OnNext(Frame);
        }

        /// <summary>Делает запрос по указанному порту</summary>
        /// <typeparam name="TFrame">Тип сообщения</typeparam>
        /// <typeparam name="TOptions">Тип опций порта</typeparam>
        /// <param name="Port">Порт для запроса</param>
        /// <param name="RequestFrame">Запрос</param>
        /// <returns>Ответ</returns>
        public static TFrame Request<TFrame, TOptions>(this IPort<TFrame, TOptions> Port, TFrame RequestFrame)
            where TOptions : PortOptions<TFrame>
        {
            return Port.Request(RequestFrame, flow => flow.First());
        }

        /// <summary>Делает запрос по указанному порту</summary>
        /// <typeparam name="TFrame">Тип сообщения</typeparam>
        /// <typeparam name="TOptions">Тип опций порта</typeparam>
        /// <param name="Port">Порт для запроса</param>
        /// <param name="RequestFrame">Запрос</param>
        /// <param name="Timeout">Таймаут запроса</param>
        /// <returns>Ответ</returns>
        public static TFrame Request<TFrame, TOptions>(this IPort<TFrame, TOptions> Port, TFrame RequestFrame, TimeSpan Timeout)
            where TOptions : PortOptions<TFrame>
        {
            return Port.Request(RequestFrame, flow => flow.Timeout(Timeout).First());
        }

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
            return Request(Port.Rx, Port.Tx, Port.Options.ProducesLoopback, Port.Options.LoopbackInspector, RequestFrame, ResultSelector);
        }

        /// <summary>
        ///     Делает запрос через указанные потоки входящих (<paramref name="Rx" />) и исходящий сообщений (
        ///     <paramref name="Tx" />)
        /// </summary>
        /// <typeparam name="TFrame">Тип сообщения</typeparam>
        /// <param name="LoopbackInspector">
        ///     Инспектор Loopback-сообщений. Не используется, если <paramref name="Loopbacked" /> ==
        ///     null
        /// </param>
        /// <param name="RequestFrame">Запрос</param>
        /// <param name="Rx">Поток приёма</param>
        /// <param name="Tx">Поток отправки</param>
        /// <param name="Loopbacked">Идут ли Loopback сообщения в <paramref name="Rx" /> поток.</param>
        /// <param name="Timeout">Время ожидания ответа  на запрос</param>
        /// <returns>Ответ на запрос</returns>
        public static TFrame Request<TFrame>(IObservable<TFrame> Rx, IObserver<TFrame> Tx,
                                             bool Loopbacked, ILoopbackInspector<TFrame> LoopbackInspector,
                                             TFrame RequestFrame, TimeSpan Timeout)
        {
            return Request(Rx, Tx, Loopbacked, LoopbackInspector, RequestFrame, flow => flow.Timeout(Timeout).First());
        }

        /// <summary>
        ///     Делает запрос через указанные потоки входящих (<paramref name="Rx" />) и исходящий сообщений (
        ///     <paramref name="Tx" />)
        /// </summary>
        /// <remarks>На вход <paramref name="ResultSelector" /> подаётся поток сообщений, полученных после отправки запроса</remarks>
        /// <typeparam name="TFrame">Тип сообщения</typeparam>
        /// <typeparam name="TResult">Тип возвращаемого результата</typeparam>
        /// <param name="LoopbackInspector">
        ///     Инспектор Loopback-сообщений. Не используется, если <paramref name="Loopbacked" /> ==
        ///     null
        /// </param>
        /// <param name="RequestFrame">Запрос</param>
        /// <param name="ResultSelector">Селектор результата запроса</param>
        /// <param name="Rx">Поток приёма</param>
        /// <param name="Tx">Поток отправки</param>
        /// <param name="Loopbacked">Идут ли Loopback сообщения в <paramref name="Rx" /> поток.</param>
        /// <returns>Ответ на запрос, обработанный селектором <paramref name="ResultSelector" /></returns>
        public static TResult Request<TFrame, TResult>(IObservable<TFrame> Rx, IObserver<TFrame> Tx,
                                                       bool Loopbacked, ILoopbackInspector<TFrame> LoopbackInspector,
                                                       TFrame RequestFrame, Func<IObservable<TFrame>, TResult> ResultSelector)
        {
            IConnectableObservable<TFrame> flow = Rx.Replay();
            using (flow.Connect())
            {
                Tx.OnNext(RequestFrame);
                IObservable<TFrame> flowAfterRequest;

                if (Loopbacked)
                {
                    // Если портом поддерживаются Loopback-пакеты, пропускаем всё, что успели принять до запроса
                    ILoopbackInspector<TFrame> loopbackInspector = LoopbackInspector;
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
