using System.Runtime.InteropServices;

namespace SocketCanWorking.Lib
{
    /// <summary>Оболочка над функциями, экспортируемыми из SocketCan.</summary>
    public static unsafe class SocketCanLib
    {
        /// <summary>Имя библиотеки-связки с SocketCan.</summary>
        public const string SocketCanLibraryName = "libSocketCanLib.so.2";

        /// <summary>Открывает сокет.</summary>
        /// <param name="InterfaceName">Имя сокета в виде c-строки.</param>
        /// <param name="TxBuffSize">Размер буфера исходящих сообщений</param>
        /// <param name="RxBuffSize">Размер буфера входящих сообщений</param>
        /// <returns>
        ///     <para>В случае успеха возвращает неотрицательный хендлер сокета.</para>
        ///     <para>При ошибке возращает код ошибки в формате:</para>
        ///     <list type="table">
        ///         <item>
        ///             <term>-100*errno</term>
        ///             <description>для ошибок в socket()</description>
        ///         </item>
        ///         <item>
        ///             <term>-10000*errno</term>
        ///             <description>для ошибок в ioctl()</description>
        ///         </item>
        ///         <item>
        ///             <term>-1000000*errno</term>
        ///             <description>для ошибов в bind()</description>
        ///         </item>
        ///     </list>
        /// </returns>
        [DllImport(SocketCanLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SocketOpen(byte[] InterfaceName, int TxBuffSize, int RxBuffSize);

        /// <summary>Закрывает сокет.</summary>
        /// <param name="Number">Номер сокета.</param>
        /// <returns>Антон придумает.</returns>
        [DllImport(SocketCanLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SocketClose(int Number);

        /// <summary>Ставит сообщения в очередь SocketCan на отправку.</summary>
        /// <remarks>Если очередь свободна, то не блокирует. Иначе блокируется до освобождения места в очереди.</remarks>
        /// <param name="Number">Номер сокета</param>
        /// <param name="Frame">Указатель на первое отправляемое сообщение</param>
        /// <param name="FramesCount">Количество отправляемых сообщений</param>
        /// <returns>Возвращает 1 в случае успеха и отрицательный код ошибки при ошибке.</returns>
        [DllImport(SocketCanLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SocketWrite(int Number, SocketCanFdFrame* Frame, int FramesCount);

        /// <summary>Читает сообщения из входящего буфера сокета.</summary>
        /// <param name="Number">Номер сокета.</param>
        /// <param name="Bags">Указатель на место, в которое будут помещены прочитанные сообщения.</param>
        /// <param name="BagsCount">Количество принимаемых сообщений.</param>
        /// <param name="Timeout">Таймаут ожидания сообщений в милисекундах (0 - до конца времён).</param>
        /// <remarks>При отсутствии сообщений в буфере блокируется до появления первого сообщения или истечения
        ///     <paramref name="Timeout" />. При наличии сообщений читает их и записывает в <paramref name="Bags" />.</remarks>
        /// <returns>
        ///     Количество принятых сообщений или код ошибки (см. таблицу ниже)
        ///     <list type="table">
        ///         <item>
        ///             <term>N >= 0</term>
        ///             <description>Количество принятых сообщений (не больше BagsNumber).</description>
        ///         </item>
        ///         <item>
        ///             <term>-1</term>
        ///             <description>Сокет закрыт</description>
        ///         </item>
        ///         <item>
        ///             <term>-255</term>
        ///             <description>Неизвестная ошибка</description>
        ///         </item>
        ///     </list>
        /// </returns>
        [DllImport(SocketCanLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SocketRead(int Number, FrameBag* Bags, uint BagsCount, int Timeout);

        /// <summary>Очищает буфер принятых сообщений сокета</summary>
        /// <remarks>Функция блокирующая</remarks>
        /// <param name="SocketNumber">Номер сокета, в котором требуется отчистить буфер входящих сообщений</param>
        /// <returns>При успехе возвращает 0, в случае ошибки отрицательный код ошибки</returns>
        [DllImport(SocketCanLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SocketFlushInBuffer(int SocketNumber);
    }
}
