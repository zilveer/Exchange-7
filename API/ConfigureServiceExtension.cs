using DataAccess.UnitOfWork;
using Entity;
using Infrastructure;
using Logic;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace API
{
    public static class ConfigureServiceExtension
    {
        public static void AddBusinessServices(this IServiceCollection services)
        {
            services.AddSingleton<IRedisCache>((serviceProvider) => { return new RedisCache("127.0.0.1:6379"); });
            services.AddSingleton<IGlobalQueryFilterRegisterer, GlobalQueryFilterRegisterer>();
            services.AddSingleton<IQueueProducer>((servicerProvider) => { return new QueueProducer("engine-in", ExchangeType.Fanout, "engine-reader", "#", "localhost", "/", "guest", "guest"); });
            services.AddScoped<ExchangeContext>(x => new ExchangeContext(new GlobalQueryFilterRegisterer(), "Host=localhost;Database=Exchange;Username=postgres;Password=root"));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IReadOnlyContext, ReadOnlyContext>();
            services.AddScoped<IMarketService, MarketService>();
            services.AddScoped<IBalanceService, BalanceService>();
            services.AddScoped<ICurrencyService, CurrencyService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IUserService, UserService>();
        }
    }
}
