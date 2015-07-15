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
        public static void Send<TFrame>(this IPort<TFrame> Port, TFrame Frame) { Port.Tx.OnNext(Frame); }

        /// <summary>Делает запрос по указанному порту</summary>
        /// <typeparam name="TFrame">Тип сообщения</typeparam>
        /// <param name="Port">Порт для запроса</param>
        /// <param name="RequestFrame">Запрос</param>
        /// <returns>Ответ</returns>
        public static TFrame Request<TFrame>(this IPort<TFrame> Port, TFrame RequestFrame)
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

                TFrame ans = flowAfterRequest.First();
                return ans;
            }
        }
    }
}
