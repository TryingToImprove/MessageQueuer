
namespace MessageQueuer
{
    public interface IMqReciever<T>
    {
        void Invoke(T message);
    }
}