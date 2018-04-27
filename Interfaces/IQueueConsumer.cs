namespace Interfaces
{
    public interface IQueueConsumer
    {
        void Consume(IMessageRecieved messageRecieved);
    }
}
