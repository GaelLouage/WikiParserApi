using Infra.Classes;
using Infra.Interfaces;
using Infra.Services.Classes;
using Infra.Services.Interfaces;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using System.Threading.RateLimiting;
using WikiParserApi.Bootstrapper;

var builder = WebApplication.CreateBuilder(args);

#region Services
builder.Services.RateLimiterRegistration();
builder.Services.AddHealthChecks();
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.SerilogConfiguration();
builder.Services.AddMemoryCache();
builder.Services.AddHealthChecks();
builder.Services.ScopesRegistration(builder);
#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthorization();

app.MapControllers();

app.Run();
