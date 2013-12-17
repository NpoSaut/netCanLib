namespace Communications.Can
{
    public interface ICanSocketSource : ISocketSource<ICanSocket>
    {
        /// <summary>
        /// Открывает CAN-сокет, способный отфильтровывать на входе все фреймы с дескрипторами, не указанными в фильтре
        /// </summary>
        /// <param name="FilterDescriptors">Принимаемые дескрипторы. Остальные будут отфильтрованы</param>
        ICanSocket OpenSocket(params int[] FilterDescriptors);
    }
}