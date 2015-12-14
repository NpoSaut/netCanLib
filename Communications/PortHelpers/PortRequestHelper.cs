using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using Communications.Options;

namespace Communications.PortHelpers
{
    public static class PortRequestHelper
    {
        #region Перегрузки

        /// <summary>Делает запрос по указанному порту</summary>
        /// <typeparam name="TFrame">Тип сообщения</typeparam>
        /// <typeparam name="TOptions">Тип опций порта</typeparam>
        /// <param name="Port">Порт для запроса</param>
        /// <param name="RequestFrame">Запрос</param>
        /// <returns>Ответ</returns>
        public static TFrame Request<TFrame, TOptions>(this IPort<TFrame, TOptions> Port, TFrame RequestFrame)
            where TOptions : IPortOptions<TFrame>
        {
            return Request(Port, RequestFrame, CancellationToken.None);
        }

        /// <summary>Делает запрос по указанному порту</summary>
        /// <typeparam name="TFrame">Тип сообщения</typeparam>
        /// <typeparam name="TOptions">Тип опций порта</typeparam>
        /// <param name="Port">Порт для запроса</param>
        /// <param name="RequestFrame">Запрос</param>
        /// <param name="ResponseTimeout">Таймаут ожидания начала ответа на запрос</param>
        /// <returns>Ответ</returns>
        public static TFrame Request<TFrame, TOptions>(this IPort<TFrame, TOptions> Port, TFrame RequestFrame, TimeSpan ResponseTimeout)
            where TOptions : IPortOptions<TFrame>
        {
            return Request(Port, RequestFrame, ResponseTimeout, TimeSpan.FromMilliseconds(Timeout.Infinite));
        }

        /// <summary>Делает запрос по указанному порту</summary>
        /// <typeparam name="TFrame">Тип сообщения</typeparam>
        /// <typeparam name="TOptions">Тип опций порта</typeparam>
        /// <param name="Port">Порт для запроса</param>
        /// <param name="RequestFrame">Запрос</param>
        /// <param name="ResponseTimeout">Таймаут ожидания начала ответа на запрос</param>
        /// <param name="TransactionTimeout">Таймаут ожидания завершения транзакции</param>
        /// <returns>Ответ</returns>
        public static TFrame Request<TFrame, TOptions>(this IPort<TFrame, TOptions> Port, TFrame RequestFrame, TimeSpan ResponseTimeout,
                                                       TimeSpan TransactionTimeout)
            where TOptions : IPortOptions<TFrame>
        {
            return Request(Port, RequestFrame, ResponseTimeout, TransactionTimeout, CancellationToken.None);
        }

        /// <summary>Делает запрос по указанному порту</summary>
        /// <typeparam name="TFrame">Тип сообщения</typeparam>
        /// <typeparam name="TOptions">Тип опций порта</typeparam>
        /// <param name="Port">Порт для запроса</param>
        /// <param name="RequestFrame">Запрос</param>
        /// <param name="CancellationToken">Токен отмены</param>
        /// <returns>Ответ</returns>
        public static TFrame Request<TFrame, TOptions>(this IPort<TFrame, TOptions> Port, TFrame RequestFrame, CancellationToken CancellationToken)
            where TOptions : IPortOptions<TFrame>
        {
            return Request(Port, RequestFrame, TimeSpan.FromMilliseconds(Timeout.Infinite), TimeSpan.FromMilliseconds(Timeout.Infinite), CancellationToken);
        }

        /// <summary>Делает запрос по указанному порту</summary>
        /// <typeparam name="TFrame">Тип сообщения</typeparam>
        /// <typeparam name="TOptions">Тип опций порта</typeparam>
        /// <param name="Port">Порт для запроса</param>
        /// <param name="RequestFrame">Запрос</param>
        /// <param name="ResponseTimeout">Таймаут ожидания начала ответа на запрос</param>
        /// <param name="CancellationToken">Токен отмены</param>
        /// <returns>Ответ</returns>
        public static TFrame Request<TFrame, TOptions>(this IPort<TFrame, TOptions> Port, TFrame RequestFrame, TimeSpan ResponseTimeout,
                                                       CancellationToken CancellationToken)
            where TOptions : IPortOptions<TFrame>
        {
            return Request(Port, RequestFrame, ResponseTimeout, TimeSpan.FromMilliseconds(Timeout.Infinite), CancellationToken);
        }

        /// <summary>Делает запрос по указанному порту</summary>
        /// <typeparam name="TFrame">Тип сообщения</typeparam>
        /// <typeparam name="TOptions">Тип опций порта</typeparam>
        /// <param name="Port">Порт для запроса</param>
        /// <param name="RequestFrame">Запрос</param>
        /// <param name="ResponseTimeout">Таймаут ожидания начала ответа на запрос</param>
        /// <param name="TransactionTimeout">Таймаут ожидания завершения транзакции</param>
        /// <param name="CancellationToken">Токен отмены</param>
        /// <returns>Ответ</returns>
        public static TFrame Request<TFrame, TOptions>(this IPort<TFrame, TOptions> Port, TFrame RequestFrame, TimeSpan ResponseTimeout,
                                                       TimeSpan TransactionTimeout, CancellationToken CancellationToken)
            where TOptions : IPortOptions<TFrame>
        {
            return Request(Port, RequestFrame, ResponseTimeout, TransactionTimeout, CancellationToken, flow => flow.First());
        }

        #endregion

        /// <summary>Делает запрос по указанному порту</summary>
        /// <remarks>На вход <paramref name="ResultSelector" /> подаётся поток сообщений, полученных после отправки запроса</remarks>
        /// <typeparam name="TFrame">Тип сообщения</typeparam>
        /// <typeparam name="TResult">Тип возвращаемого результата</typeparam>
        /// <typeparam name="TOptions">Тип опций порта</typeparam>
        /// <param name="Port">Порт для запроса</param>
        /// <param name="RequestFrame">Запрос</param>
        /// <param name="ResponseTimeout">Таймаут ожидания начала ответа на запрос</param>
        /// <param name="TransactionTimeout">Таймаут ожидания завершения транзакции</param>
        /// <param name="CancellationToken">Токен отмены</param>
        /// <param name="ResultSelector">Селектор результата запроса</param>
        /// <returns>Ответ на запрос, обработанный селектором <paramref name="ResultSelector" /></returns>
        public static TResult Request<TFrame, TOptions, TResult>(this IPort<TFrame, TOptions> Port, TFrame RequestFrame,
                                                                 TimeSpan ResponseTimeout, TimeSpan TransactionTimeout,
                                                                 CancellationToken CancellationToken,
                                                                 Func<IObservable<TFrame>, TResult> ResultSelector)
            where TOptions : IPortOptions<TFrame>
        {
            var cancellationFlow = Observable.Create<TFrame>(observer => CancellationToken.Register(() => observer.OnError(new OperationCanceledException())));
            IConnectableObservable<TFrame> flow = Port.Rx.Timeout(ResponseTimeout.RxInfinite())
                                                      .Select(t => t.Wait(TransactionTimeout, CancellationToken))
                                                      .Merge(cancellationFlow)
                                                      .Replay();
            using (flow.Connect())
            {
                Port.BeginSend(RequestFrame).Wait(TransactionTimeout, CancellationToken);

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
