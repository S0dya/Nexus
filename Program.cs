using Microsoft.EntityFrameworkCore;
using Nexus.Database;
using Nexus.Infrastructure.DependencyInjection;
using Nexus.Infrastructure.DependencyInjection.RateLimiting;
using Nexus.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddSwagger()
    .AddHttpContextAccessor()
    .AddProjectServices(builder.Configuration)
    .AddAuth(builder.Configuration)
    .AddDatabase(builder.Configuration)
    .AddRateLimiting(builder.Configuration)
    .TimerAddCors();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var port = Environment.GetEnvironmentVariable("PORT") ?? "10001";

builder.WebHost.UseUrls($"http://*:{port}");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // app.MapOpenApi();
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

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<LastOnlineMiddleware>();

app.MapControllers();

app.Run();