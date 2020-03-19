namespace Infrastructure
{
    interface IQueueConsumer
    {
        public byte[] Consume();
    }
}
