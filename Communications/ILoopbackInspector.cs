using System;

namespace Communications
{
    /// <summary>Инструмент, позволяющий определить, является ли один пакет Loopback-пакетом для другого</summary>
    /// <typeparam name="TFrame">Тип пакета</typeparam>
    public interface ILoopbackInspector<TFrame>
    {
        /// <summary>Определяет, является ли один пакет Loopback-пакетом для другого</summary>
        /// <param name="Candidate">Проверяемый пакет</param>
        /// <param name="Original">Оригинальный пакет</param>
        /// <returns>True, если <paramref name="Candidate" /> является Loopback-пакетом для <paramref name="Original" /></returns>
        Boolean IsLoopback(TFrame Candidate, TFrame Original);
    }
}
