using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Communications.Tools
{
    /// <summary>Содержит инструменты для перебора коллекции с подавлением ошибок</summary>
    internal static class TryCatchEnumerationHelper
    {
        /// <summary>Перебирает последовательность, подавляя указанные типы исключений</summary>
        /// <remarks>
        ///     Если при переборе исходной последовательности возникает исключения одного из типов, указанных в
        ///     <paramref name="SuppressedExceptionTypes" />, исключение подавляется а перечисление последовательности
        ///     прекращается.
        /// </remarks>
        /// <typeparam name="T">Тип элемента коллекции</typeparam>
        /// <param name="Enumerable">Исходная последовательность</param>
        /// <param name="SuppressedExceptionTypes">Типы подавляемых исключений</param>
        /// <returns>
        ///     Исходная последовательность, перебираемая до её окончания или возникновения исключения, указанного в
        ///     <paramref name="SuppressedExceptionTypes" />
        /// </returns>
        public static IEnumerable<T> SuppressExceptions<T>(this IEnumerable<T> Enumerable, params Type[] SuppressedExceptionTypes)
        {
            if (!SuppressedExceptionTypes.All(t => t == typeof (Exception) || t.IsSubclassOf(typeof (Exception))))
            {
                throw new ArgumentException(
                    string.Format("Все типы, передаваемые в параметре SuppressedExceptionTypes должны быть наследниками {0}", typeof (Exception)),
                    "SuppressedExceptionTypes");
            }
            return new TryCatchEnumerableDecorator<T>(Enumerable, new HashSet<Type>(SuppressedExceptionTypes));
        }

        /// <summary>Перебирает последовательность, подавляя указанные типы исключений</summary>
        /// <remarks>
        ///     Если при переборе исходной последовательности возникает исключение типа <typeparamref name="TException" />,
        ///     исключение подавляется а перечисление последовательности прекращается.
        /// </remarks>
        /// <typeparam name="TElement">Тип элемента коллекции</typeparam>
        /// <typeparam name="TException">Тип подавляемого исключения</typeparam>
        /// <param name="Enumerable">Исходная последовательность</param>
        /// <returns>
        ///     Исходная последовательность, перебираемая до её окончания или возникновения исключения, указанного в
        ///     <typeparamref name="TException" />
        /// </returns>
        public static IEnumerable<TElement> SuppressExceptions<TElement, TException>(this IEnumerable<TElement> Enumerable)
            where TException : Exception
        {
            return SuppressExceptions(Enumerable, new[] { typeof (TException) });
        }

        /// <summary>Последовательность, перечисляемая до возникновения исключения</summary>
        /// <typeparam name="T">Тип элемента последовательности</typeparam>
        private class TryCatchEnumerableDecorator<T> : IEnumerable<T>
        {
            private readonly IEnumerable<T> _baseEnumerable;
            private readonly ISet<Type> _suppressedExceptions;

            public TryCatchEnumerableDecorator(IEnumerable<T> BaseEnumerable, ISet<Type> SuppressedExceptions)
            {
                _baseEnumerable = BaseEnumerable;
                _suppressedExceptions = SuppressedExceptions;
            }

            /// <summary>Возвращает перечислитель, выполняющий итерацию в коллекции.</summary>
            /// <returns>
            ///     Интерфейс <see cref="T:System.Collections.Generic.IEnumerator`1" />, который может использоваться для перебора
            ///     элементов коллекции.
            /// </returns>
            public IEnumerator<T> GetEnumerator()
            {
                return new TryCatchEnumeratorDecorator<T>(_baseEnumerable.GetEnumerator(), _suppressedExceptions);
            }

            /// <summary>Возвращает перечислитель, который осуществляет перебор элементов коллекции.</summary>
            /// <returns>
            ///     Объект <see cref="T:System.Collections.IEnumerator" />, который может использоваться для перебора элементов
            ///     коллекции.
            /// </returns>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            private class TryCatchEnumeratorDecorator<T> : IEnumerator<T>
            {
                private readonly IEnumerator<T> _baseEnumerator;
                private readonly ISet<Type> _suppressedExceptions;

                public TryCatchEnumeratorDecorator(IEnumerator<T> BaseEnumerator, ISet<Type> SuppressedExceptions)
                {
                    _baseEnumerator = BaseEnumerator;
                    _suppressedExceptions = SuppressedExceptions;
                }

                /// <summary>
                ///     Выполняет определяемые приложением задачи, связанные с удалением, высвобождением или сбросом неуправляемых
                ///     ресурсов.
                /// </summary>
                public void Dispose()
                {
                    _baseEnumerator.Dispose();
                }

                /// <summary>Перемещает перечислитель к следующему элементу коллекции.</summary>
                /// <returns>
                ///     Значение true, если перечислитель был успешно перемещен к следующему элементу; значение false, если
                ///     перечислитель достиг конца коллекции.
                /// </returns>
                /// <exception cref="T:System.InvalidOperationException">Коллекция была изменена после создания перечислителя. </exception>
                public bool MoveNext()
                {
                    try
                    {
                        return _baseEnumerator.MoveNext();
                    }
                    catch (Exception e)
                    {
                        if (_suppressedExceptions.Contains(e.GetType())) return false;
                        throw;
                    }
                }

                /// <summary>Устанавливает перечислитель в его начальное положение, перед первым элементом коллекции.</summary>
                /// <exception cref="T:System.InvalidOperationException">Коллекция была изменена после создания перечислителя. </exception>
                public void Reset()
                {
                    _baseEnumerator.Reset();
                }

                /// <summary>Получает элемент коллекции, соответствующий текущей позиции перечислителя.</summary>
                /// <returns>Элемент коллекции, соответствующий текущей позиции перечислителя.</returns>
                public T Current
                {
                    get { return _baseEnumerator.Current; }
                }

                /// <summary>Получает текущий элемент в коллекции.</summary>
                /// <returns>Текущий элемент в коллекции.</returns>
                /// <exception cref="T:System.InvalidOperationException">
                ///     Перечислитель помещается перед первым элементом коллекции или
                ///     после последнего элемента.
                /// </exception>
                object IEnumerator.Current
                {
                    get { return _baseEnumerator.Current; }
                }
            }
        }
    }
}
