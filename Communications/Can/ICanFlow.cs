using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Communications.Can
{
    public interface ICanFlow
    {
        /// <summary>Список дескрипторов, отлавливаемых в данный поток</summary>
        ReadOnlyCollection<int> Descriptors { get; }

        /// <summary>Отправляет Can-сообщение в поток</summary>
        void Send(CanFrame Frame, bool ClearBeforeSend = false);

        /// <summary>Отправляет несколько Can-сообщений в поток</summary>
        void Send(IList<CanFrame> Frames, bool ClearBeforeSend = false);

        /// <summary>Блокирующе считывает последовательность всех фреймов, приходящих в данный буфер</summary>
        /// <returns>Последовательность отловленных кадров</returns>
        /// <remarks>
        ///     Поскольку функция блокирующая и считывает _все_ приходящие кадры, выход из этого перечисления не предусмотрен.
        ///     Для правильной работы следует пользоваться лишь методами вроде методов, извлекающих N первых членов
        ///     последовательности (First, Take, TakeWhile, ...)
        /// </remarks>
        IEnumerable<CanFrame> Read();

        /// <summary>
        ///     Блокирующе считывает последовательность всех фреймов, приходящих в данный буфер до тех пор, пока в течение
        ///     указанного интервала приходит хоть одно сообщение
        /// </summary>
        /// <param name="Timeout">Время, по истечении которого чтение следующих пакетов прекратится</param>
        /// <param name="ThrowExceptionOnTimeout">
        ///     Следует ли пробрасывать TimeoutException по истечении таймаута (если нет - то
        ///     чтение пакетов просто прекратится)
        /// </param>
        /// <returns>Последовательность отловленных кадров</returns>
        /// <remarks>
        ///     Можно использовать для отлова серий подряд идущих сообщений (когда другая серия отделяется по времени).
        ///     Параметр ThrowExceptionOnTimeout следует использовать, если сообщение гарантированно должно поступить и его
        ///     задержка является признаком ошибки
        /// </remarks>
        IEnumerable<CanFrame> Read(TimeSpan Timeout, Boolean ThrowExceptionOnTimeout = false);

        void Clear();
    }
}
