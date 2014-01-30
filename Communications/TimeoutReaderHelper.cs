using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Communications.Exceptions;

namespace Communications
{
    /// <summary>
    /// Помогает производить считывание и обработку потока дейтаграмм из сокета с отслеживанием таймаута
    /// </summary>
    public static class TimeoutReaderHelper
    {
        /// <summary>
        /// Производит считывание и обработку потока дейтаграмм, выполняя отслеживание таймаута операции на "дальнем конце" цепочки обработки
        /// </summary>
        /// <typeparam name="TIn">Тип дейтаграммы, исходящей из сокета</typeparam>
        /// <typeparam name="TOut">Тип результата цепочки обработки</typeparam>
        /// <param name="Socket">Сокет, из которого производится чтение</param>
        /// <param name="OutputSelector">Цепочка фильтров или обработок</param>
        /// <param name="Timeout">Время ожидания следующего сообщения в выходной цепочке</param>
        /// <param name="ThrowExceptionOnTimeout">Стоит ли выбрасывать исключение при превышении таймаута</param>
        /// <returns>Цепочка обработанных дейтаграмм, с временем между ними, не превышающим <paramref name="Timeout"/></returns>\
        /// <exception cref="SocketReadTimeoutException">Выбрасывается при превышении времени ожидания следующей дейтаграммы, если <paramref name="ThrowExceptionOnTimeout"/> == true</exception>
        public static IEnumerable<TOut> ReadWithTimeout<TIn, TOut>(this ISocket<TIn> Socket,
                                                                   Func<IEnumerable<TIn>, IEnumerable<TOut>>
                                                                       OutputSelector,
                                                                   TimeSpan Timeout,
                                                                   bool ThrowExceptionOnTimeout = false)
        {
            return new TimeoutReader<TIn, TOut>(Socket, OutputSelector).Read(Timeout, ThrowExceptionOnTimeout);
        }


        private class TimeoutReader<TIn, TOut>
        {
            private ISocket<TIn> Socket { get; set; }
            private Func<IEnumerable<TIn>, IEnumerable<TOut>> OutputSelector { get; set; }

            public TimeoutReader(ISocket<TIn> Socket,
                                 Func<IEnumerable<TIn>, IEnumerable<TOut>> OutputSelector)
            {
                this.Socket = Socket;
                this.OutputSelector = OutputSelector;
            }

            public IEnumerable<TOut> Read(TimeSpan Timeout, Boolean ThrowExceptionOnTimeOut = false)
            {
                var sw = new Stopwatch();
                sw.Start();
                var checkedInputs = FastCheck(Socket.Receive(Timeout, ThrowExceptionOnTimeOut),
                                              sw, Timeout, ThrowExceptionOnTimeOut);
                var outputs = OutputSelector(checkedInputs);
                foreach (var output in outputs)
                {
                    if (sw.Elapsed < Timeout)
                    {
                        yield return output;
                        sw.Reset();
                    }
                    else if (ThrowExceptionOnTimeOut)
                        throw new SocketReadTimeoutException("Ожидаемое сообщение было получено после истечения таймаута");
                    else yield break;
                }
            }

            private static IEnumerable<TIn> FastCheck(IEnumerable<TIn> inputs, Stopwatch sw, TimeSpan Timeout,
                                                      Boolean ThrowExceptionOnTimeOut = false)
            {
                foreach (var input in inputs)
                {
                    if (sw.Elapsed < Timeout) yield return input;
                    else if (ThrowExceptionOnTimeOut) throw new SocketReadTimeoutException(
                            "При чтении из сокета приходили другие сообщения, но ожидаемое сообщение всё равно не было получено вовремя");
                    else yield break;
                }
            }
        }
    }
}
