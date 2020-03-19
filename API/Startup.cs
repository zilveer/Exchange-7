using API.Middleware;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSwag;
using NSwag.AspNetCore;
using NSwag.Generation.Processors.Security;
using System.Collections.Generic;
using System.Linq;

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
                    builder.RequireScope("orders:read", "orders:write");
                });

                options.AddPolicy("post-orders", builder =>
                {
                    builder.RequireScope("orders:write");
                });

                options.AddPolicy("read-balance", builder =>
                {
                    builder.RequireScope("balance:read");
                });
            });
            services.AddSwaggerDocument(config =>
            {
                config.Title = "Exchange API";
                config.Version = "0.9";
                config.OperationProcessors.Add(new AddRequiredHeaderParameter());
                config.AddSecurity("bearer", Enumerable.Empty<string>(), new OpenApiSecurityScheme
                {
                    Type = OpenApiSecuritySchemeType.OAuth2,
                    Flow = OpenApiOAuth2Flow.Implicit,
                    AuthorizationUrl = "https://localhost:44326/connect/authorize",
                    Scopes = new Dictionary<string, string>()
                    {
                        { "orders:read", "read orders" },
                        { "orders:write", "place and cancel orders" },
                        { "balance:read", "read balance" },
                    }
                });
                config.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("bearer"));
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
            app.UseSwaggerUi3(settings =>
            {
                settings.OAuth2Client = new OAuth2ClientSettings
                {
                    ClientId = "brvesqplaedoadrhklar",
                    ClientSecret = "sbowplazhpgtbtrnbaswxquzbazggkxu",
                    AppName = "WebAppAuth",
                };
            });
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
