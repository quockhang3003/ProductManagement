
using ProductManagement.BackgroundWorker;
using ProductManagement.Infrastructure;
using ProductManagement.Infrastructure.Messaging;

var builder = Host.CreateApplicationBuilder(args);

// Configure RabbitMQ Settings
builder.Services.Configure<RabbitMQSettings>(
    builder.Configuration.GetSection("RabbitMQ"));

// Add Infrastructure (Database, Messaging, Caching)
var redisConnection = builder.Configuration.GetConnectionString("Redis");
builder.Services.AddInfrastructure(redisConnection);

// Add Application Services
builder.Services.AddApplicationServices();

// Add Hosted Services (Background Workers)
builder.Services.AddHostedService<ProductEventWorker>();
builder.Services.AddHostedService<OrderEventWorker>();
builder.Services.AddHostedService<InventoryEventWorker>();
builder.Services.AddHostedService<PaymentEventWorker>();
builder.Services.AddHostedService<PromotionEventWorker>();
var host = builder.Build();
host.Run();