using Entity;
using Infrastructure;
using OrderMatcher;
using System;
using System.Linq;

namespace OrderMatcherInitializer
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                do
                {
                    Console.WriteLine("Type \"INITIALIZE\" (in caps) to initialize process.");
                } while (Console.ReadLine() != "INITIALIZE");

                int marketId;
                do
                {
                    Console.Write("Enter Market Id > ");

                } while (!int.TryParse(Console.ReadLine(), out marketId));
                Console.WriteLine(marketId);

                do
                {
                    Console.Write("Re-Enter Market Id > ");

                } while (!int.TryParse(Console.ReadLine(), out var f) || f != marketId);
                Console.WriteLine(marketId);

                QueueProducer queueProducer = new QueueProducer("engine-in", "fanout", "engine-reader", "#", "localhost", "/", "guest", "guest");
                var dbContext = new ExchangeContext(new GlobalQueryFilterRegisterer(), "Host=localhost;Database=Exchange;Username=postgres;Password=root");
                var orders = dbContext.Orders.Where(x => x.IsDeleted == false && x.MarketId == marketId && (x.OrderStatus == Entity.Partials.OrderStatus.Accepted || x.OrderStatus == Entity.Partials.OrderStatus.Received)).OrderBy(x => x.CreatedOn).ToList();
                foreach (var order in orders)
                {

                    var orderWrapper = new OrderWrapper();
                    orderWrapper.Order = new Order();

                    orderWrapper.Order.OrderId = order.Id;
                    orderWrapper.Order.IsBuy = order.Side;
                    orderWrapper.Order.Price = order.Rate;
                    orderWrapper.Order.OpenQuantity = order.QuantityRemaining;
                    orderWrapper.Order.IsStop = order.StopRate != 0;
                    orderWrapper.OrderCondition = (OrderCondition)((byte)order.OrderCondition);
                    orderWrapper.StopPrice = order.StopRate;
                    orderWrapper.TipQuantity = order.IcebergQuantity > 0 ? order.Quantity : 0;
                    orderWrapper.TotalQuantity = order.IcebergQuantity ?? 0;
                    if (order.CancelOn.HasValue)
                    {
                        DateTime Jan1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                        orderWrapper.Order.CancelOn = (int)order.CancelOn.Value.Subtract(Jan1970).TotalSeconds;
                    }

                    var bytes = OrderSerializer.Serialize(orderWrapper);
                    queueProducer.Produce(bytes);
                }
                queueProducer.Flush();
                queueProducer.Close();
                Console.WriteLine("Finished..");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.ReadLine();
        }
    }
}
