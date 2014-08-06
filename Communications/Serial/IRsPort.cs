namespace Communications.Serial
{
    /// <summary>Последовательный порт</summary>
    public interface IRsPort : IPort<IRsSocket>, IBaudRateable { }
}
