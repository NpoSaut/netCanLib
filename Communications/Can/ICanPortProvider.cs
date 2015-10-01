namespace Communications.Can
{
    public interface ICanPortProvider
    {
        ICanPort OpenPort();
    }
}
