using System;

namespace Communications.Appi.Timeouts
{
    /// <summary>Диспетчер таймаутов</summary>
    /// <remarks>Используется для взведения и отмены времени ожидания</remarks>
    /// <typeparam name="TTimeoutInformation">Тип параметра времени ожидания. Обычно обозначает причины ожидания</typeparam>
    public interface ITimeoutManager<in TTimeoutInformation> : IDisposable
    {
        /// <summary>Взводит таймер</summary>
        /// <param name="Timeout">Время ожидания</param>
        /// <param name="Information">Информация о причине ожидания</param>
        void CockTimer(TimeSpan Timeout, TTimeoutInformation Information);

        /// <summary>Отменяет ожидание</summary>
        void DecockTimer();
    }
}
