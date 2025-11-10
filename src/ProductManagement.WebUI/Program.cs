using ProductManagement;
using ProductManagement.Infrastructure;
using ProductManagement.Infrastructure.Messaging;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using ProductManagement.WebUI.Components;

var builder = WebApplication.CreateBuilder(args);


// Razor Components (Blazor Web App .NET 8)
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(); 


builder.Services.AddControllers();

// Configure RabbitMQ Settings
builder.Services.Configure<RabbitMQSettings>(
    builder.Configuration.GetSection("RabbitMQ"));

// Add Infrastructure (Database, Messaging, Caching)
var redisConnection = builder.Configuration.GetConnectionString("Redis");
builder.Services.AddInfrastructure(redisConnection);

// Add Application Services
builder.Services.AddApplicationServices();


// Configure Rate Limiting
builder.Services.AddRateLimiter(options =>
{ 
    options.AddSlidingWindowLimiter("sliding", opt =>
    {
        opt.PermitLimit = 10;              
        opt.Window = TimeSpan.FromSeconds(5);
        opt.SegmentsPerWindow = 2;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;                
    });
    
    options.AddTokenBucketLimiter("token", opt =>
    {
        opt.TokenLimit = 5;                
        opt.TokensPerPeriod = 5;
        opt.ReplenishmentPeriod = TimeSpan.FromSeconds(10);
        opt.AutoReplenishment = true;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;                
    });
    
    options.AddConcurrencyLimiter("concurrency", opt =>
    {
        opt.PermitLimit = 5;
        opt.QueueLimit = 2;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });

    options.RejectionStatusCode = 429;
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        if (!context.HttpContext.Response.HasStarted)
        {
            await context.HttpContext.Response.WriteAsync(
                "❌ Too many requests, please slow down.", token);
        }
    };
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseRateLimiter();
app.UseAntiforgery();
app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.Run();
