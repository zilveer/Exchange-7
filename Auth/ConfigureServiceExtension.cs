using DataAccess.UnitOfWork;
using Entity;
using Infrastructure;
using Logic;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Text;
using Util;

namespace Auth
{
    public static class ConfigureServiceExtension
    {
        public static void AddBusinessServices(this IServiceCollection services)
        {
            var key = Encoding.UTF8.GetBytes("umuidmrwkfppcgkldvdtfchgbdjsyyoq").Take(32).ToArray();
            var iv = Encoding.UTF8.GetBytes("vhyngyaokusuibxh").Take(16).ToArray();

            services.AddSingleton<ITimeProvider, TimeProvider>();
            services.AddSingleton<IRedisCache>((serviceProvider) => { return new RedisCache("127.0.0.1:6379"); });
            services.AddSingleton<IGlobalQueryFilterRegisterer, GlobalQueryFilterRegisterer>();
            services.AddScoped<ExchangeContext>(x => new ExchangeContext(new GlobalQueryFilterRegisterer(), "Host=localhost;Database=Exchange;Username=postgres;Password=root"));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IReadOnlyContext, ReadOnlyContext>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>((serviceProvider) => { return new AuthenticationService(serviceProvider.GetService<IUnitOfWork>(), serviceProvider.GetService<IReadOnlyContext>(), serviceProvider.GetService<IRedisCache>(), serviceProvider.GetService<ITimeProvider>(), key, iv); });
        }
    }
}
