namespace Infrastructure
{
    public interface IQueueProducer
    {
        public void Produce(byte[] message, string routingKey = "");
    }
}
