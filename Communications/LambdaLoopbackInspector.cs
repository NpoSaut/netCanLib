using System;

namespace Communications
{
    /// <summary>
    ///     <see cref="ILoopbackInspector{TFrame}" />, действующий на основании предоставленного лямбда-выражения
    /// </summary>
    /// <typeparam name="TFrame">Тип пакета</typeparam>
    public class LambdaLoopbackInspector<TFrame> : ILoopbackInspector<TFrame>
    {
        private readonly Func<TFrame, TFrame, bool> _isLoopbackPredicate;

        /// <summary>Создаёт <see cref="ILoopbackInspector{TFrame}" />, действующий на основании предоставленного лямбда-выражения</summary>
        /// <param name="IsLoopbackPredicate">
        ///     Лямбда-выражение, первый параметр которого принимает проверяемый кадр, а второй -
        ///     оригинальный кадр
        /// </param>
        public LambdaLoopbackInspector(Func<TFrame, TFrame, bool> IsLoopbackPredicate) { _isLoopbackPredicate = IsLoopbackPredicate; }

        /// <summary>Определяет, является ли один пакет Loopback-пакетом для другого</summary>
        /// <param name="Candidate">Проверяемый пакет</param>
        /// <param name="Original">Оригинальный пакет</param>
        /// <returns>True, если <paramref name="Candidate" /> является Loopback-пакетом для <paramref name="Original" /></returns>
        public bool IsLoopback(TFrame Candidate, TFrame Original) { return _isLoopbackPredicate(Candidate, Original); }
    }
}