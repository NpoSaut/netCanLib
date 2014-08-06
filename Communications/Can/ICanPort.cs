namespace Communications.Can
{
    /// <summary>CAN-порт</summary>
    public interface ICanPort : IPort<ICanSocket>, IBaudRateable { }
}
