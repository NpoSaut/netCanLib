using Communications.Options;

namespace Communications.Can
{
    /// <summary>Опции CAN-порта</summary>
    public interface ICanPortOptions : IPortOptions<CanFrame>, IBaudRatePortOptions, IDataPortOptions { }
}
