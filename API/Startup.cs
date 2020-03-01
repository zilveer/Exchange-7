using API.Middleware;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddBusinessServices();
            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                    .AddIdentityServerAuthentication(options =>
                    {
                        options.Authority = "https://localhost:44326/";
                        options.ApiName = "Exchange";
                    });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("read-orders", builder =>
                {
                    builder.RequireScope("orders:read");
                });
            });
            services.AddSwaggerDocument(config =>
            {
                config.Title = "Exchange API";
                config.Version = "0.9";
                config.OperationProcessors.Add(new AddRequiredHeaderParameter());
            });
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseMiddleware<DuplicateRequestMiddleware>();
            app.UseRouting();
            app.UseCors(configurePolicy => configurePolicy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
            app.UseOpenApi();
            app.UseSwaggerUi3();
        }
    }
}


/* api
 *  Order/
 *      place
 *      cancel
 *      cancel all
 *      fills
 *  currency
 *  markets
 *      ticker
 *      trade history / recent-trades
 *      candles
 *      book
 *  balance
 *  send
 *  receiving address
 *  sending address
 *  transactions
 */
