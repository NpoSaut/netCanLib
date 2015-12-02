using Communications.Options;

namespace Communications
{
    /// <summary>Порт передачи бинарных данных</summary>
    /// <typeparam name="TFrame">Тип кадра</typeparam>
    /// <typeparam name="TOptions">Тип опций порта</typeparam>
    public interface IDataPort<TFrame, out TOptions> : IPort<TFrame, TOptions>
        where TFrame : IDataFrame
        where TOptions : PortOptions<TFrame>, IDataPortOptions { }

    /// <summary>Порт передачи бинарных данных</summary>
    /// <typeparam name="TFrame">Тип кадра</typeparam>
    public interface IDataPort<TFrame> : IPort<TFrame, DataPortOptions<TFrame>>
        where TFrame : IDataFrame { }
}
