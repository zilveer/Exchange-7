using Infrastructure;
using OrderMatcher;
using System;

namespace Engine
{
    class Program
    {
        static void Main(string[] args)
        {
            Core core1 = new Core();
            var consumerConfig = new QueueConfig
            {
                ExchangeName = "engine-in",
                ExchangeType = "fanout",
                QueueName = "engine-reader",
                RoutingKey = "#",
                HostName = "localhost",
                Vhost = "/",
                User = "guest",
                Password = "guest"
            };
            var producerConfig = new QueueConfig
            {
                ExchangeName = "engine-out",
                ExchangeType = "fanout",
                QueueName = "settlement",
                RoutingKey = "#",
                HostName = "localhost",
                Vhost = "/",
                User = "guest",
                Password = "guest"
            };

            core1.Start(consumerConfig, producerConfig, "output.bin", new TimeProvider(), 0, 1);

            Console.ReadLine();
            core1.Stop();
        }
    }
}
