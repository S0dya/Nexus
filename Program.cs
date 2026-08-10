using Microsoft.EntityFrameworkCore;
using Nexus.Database;
using Nexus.Infrastructure.DependencyInjection;
using Nexus.Infrastructure.DependencyInjection.RateLimiting;
using Nexus.Middlewares;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddSwagger()
    .AddHttpContextAccessor()
    .AddProjectServices(builder.Configuration)
    .AddAuth(builder.Configuration)
    .AddDatabase(builder.Configuration)
    .AddRedis(builder.Configuration)
    .AddRateLimiting(builder.Configuration)
    .TimerAddCors();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(
        "logs/nexus-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7)
    .CreateLogger();

builder.Host.UseSerilog();

var port = Environment.GetEnvironmentVariable("PORT") ?? "10001";

builder.WebHost.UseUrls($"http://*:{port}");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(app.Environment);
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

app.UseSerilogRequestLogging();

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<LastOnlineMiddleware>();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program
{
}