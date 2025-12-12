using Infra.Classes;
using Infra.Helpers;
using Infra.Interfaces;
using Infra.Services.Classes;
using Infra.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

namespace WikiParserApi.Bootstrapper
{
    public static class ServicesBootstrapper
    {
        public static void ScopesRegistration(this IServiceCollection services, WebApplicationBuilder builder)
        {
            //singleton registrations
            services.AddSingleton<IAppMetricsService, AppMetricsService>();
            //scoped registrations
            services.AddScoped<IPdfService, PdfService>();
            services.AddScoped<IMemoryCacheService, MemoryCacheService>();
            services.AddScoped<IJwtTokenService,  JwtTokenService>();
            services.AddScoped<IWikiParserService>(sp =>
            {
                var config = sp.GetRequiredService<IConfiguration>();
                var appMetricsService = sp.GetRequiredService<IAppMetricsService>();
                return new WikiParserService(config, appMetricsService);
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

        public static void AddJWToken(this WebApplicationBuilder builder)
        {
            builder.Services.AddAuthentication(cfg => {
                cfg.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                cfg.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                cfg.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x => {
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidAudience = builder.Configuration["ApplicationSettings:JWT_Audience"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["ApplicationSettings:JWT_Secret"])),
                    ValidIssuer = builder.Configuration["ApplicationSettings:JWT_Issuer"],
                    ClockSkew = TimeSpan.Zero,
                    RoleClaimType = ClaimTypes.Role
                };
            });
        }
    }
}
