using System;

namespace TradeFinalizer
{
    class Program
    {
        static void Main(string[] args)
        {
            var tradeFinalizer = new TradeFinalizer();
            var consumerConfig = new QueueConfig
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

            tradeFinalizer.Start(consumerConfig);

            Console.ReadLine();
            tradeFinalizer.Stop();
        }
    }
}
