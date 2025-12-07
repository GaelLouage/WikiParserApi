using Infra.Classes;
using Infra.Interfaces;
using Infra.Services.Classes;
using Infra.Services.Interfaces;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using System.Threading.RateLimiting;

namespace WikiParserApi.Bootstrapper
{
    public static class ServicesBootstrapper
    {
        public static void ScopesRegistration(this IServiceCollection services, WebApplicationBuilder builder)
        {
            services.AddScoped<IPdfService, PdfService>();
            services.AddScoped<IMemoryCacheService, MemoryCacheService>();

            services.AddSingleton<IWikiParserService>(x =>
            {
                return new WikiParserService(builder.Configuration);
            });
        }

        // rate limiter configuration moved to Program.cs
        public static void RateLimiterRegistration(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.AddFixedWindowLimiter("fixed", limiter =>
                {
                    limiter.Window = TimeSpan.FromMinutes(1);
                    limiter.PermitLimit = 3;
                    limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    limiter.QueueLimit = 0;
                });
            });
        }

        // Bind Serilog from configuration
        public static void SerilogConfiguration(this WebApplicationBuilder builder)
        {
    
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            builder.Host.UseSerilog();
        }

        public static void AddHealthChecks(this WebApplicationBuilder builder)
        {
            builder.Services.AddHealthChecks()
                // Basic self-check
                .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "liveness" })

                // Wikipedia availability check - replace with your actual endpoint
                .AddUrlGroup(
                    new Uri("https://localhost:7072/api/rest_v1/Wiki/Britney_Spears/0"),
                    name: "wikipedia_api",
                    failureStatus: HealthStatus.Unhealthy);

            //// Cache readiness (example for Redis)
            //.AddRedis(
            //    redisConnectionString: builder.Configuration.GetConnectionString("Redis"),
            //    name: "redis_cache",
            //    failureStatus: HealthStatus.Unhealthy);
        }
    }
}
