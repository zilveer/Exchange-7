namespace Engine
{
    public class QueueConfig
    {
        public string ExchangeName { get; set; }
        public string ExchangeType { get; set; }
        public string QueueName { get; set; }
        public string RoutingKey { get; set; }
        public string HostName { get; set; }
        public string Vhost { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
    }
}
