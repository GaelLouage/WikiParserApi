using Infra.Classes;
using Infra.Interfaces;
using Infra.Services.Classes;
using Infra.Services.Interfaces;
using Microsoft.AspNetCore.RateLimiting;
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

            services.AddScoped<IWikiParserService>(x =>
            {
                var wikiBaseUrl = builder.Configuration.GetValue<string>("WikiBaseUrl");
                return new WikiParserService(wikiBaseUrl);
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
    }
}
