namespace Communications.Appi.Encoders
{
    public interface IInterfaceCodeProvider<TLineKey>
    {
        byte GetInterfaceCode(TLineKey Interface);
    }
}
