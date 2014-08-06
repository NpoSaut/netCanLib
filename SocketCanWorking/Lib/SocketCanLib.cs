using System.Runtime.InteropServices;

namespace SocketCanWorking.Lib
{
    /// <summary>Оболочка над функциями, экспортируемыми из SocketCan.</summary>
    public static unsafe class SocketCanLib
    {
        /// <summary>Имя библиотеки-связки с SocketCan.</summary>
        public const string SocketCanLibraryName = "libSocketCanLib.so.1";

        /// <summary>Открывает сокет.</summary>
        /// <param name="InterfaceName">Имя сокета в виде c-строки.</param>
        /// <returns>Номер открытого сокета.</returns>
        [DllImport(SocketCanLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SocketOpen(byte[] InterfaceName);

        /// <summary>Закрывает сокет.</summary>
        /// <param name="Number">Номер сокета.</param>
        /// <returns>Антон придумает.</returns>
        [DllImport(SocketCanLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SocketClose(int Number);

        /// <summary>Отправляет в сокет.</summary>
        /// <param name="Number">Номер сокета.</param>
        /// <param name="Frame">Фрейм для отправки.</param>
        /// <returns>True, если фрейм успешно отправлен.</returns>
        [DllImport(SocketCanLibraryName, CallingConvention = CallingConvention.Cdecl)]
        public static extern int SocketWrite(int Number, SocketCanFdFrame* Frame);

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
    }
}
