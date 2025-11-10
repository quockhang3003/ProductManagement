using ProductManagement.Application.Messaging;
using ProductManagement.Domain.Events;

namespace ProductManagement.BackgroundWorker;

public class ProductEventWorker : BackgroundService
{
    private readonly IMessageConsumer _messageConsumer;
    private readonly ILogger<ProductEventWorker> _logger;

    public ProductEventWorker(
        IMessageConsumer messageConsumer,
        ILogger<ProductEventWorker> logger)
    {
        _messageConsumer = messageConsumer;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Product Event Worker starting at: {Time}", DateTimeOffset.Now);

        // Subscribe to product events from RabbitMQ
        _messageConsumer.Subscribe<ProductCreatedEvent>(
            "product-events",
            HandleProductCreatedAsync);

        _messageConsumer.Subscribe<ProductStockUpdatedEvent>(
            "product-events",
            HandleStockUpdatedAsync);

        _messageConsumer.Subscribe<ProductPriceChangedEvent>(
            "product-events",
            HandlePriceChangedAsync);

        _messageConsumer.StartConsuming();

        _logger.LogInformation("Product Event Worker is now listening to events...");

        return Task.CompletedTask;
    }

    private async Task HandleProductCreatedAsync(ProductCreatedEvent @event)
    {
        _logger.LogInformation(
            "✅ Product Created: {ProductId} - {Name} - Price: {Price:C} - Stock: {Stock}",
            @event.ProductId, @event.Name, @event.Price, @event.Stock);

        // TODO: Implement business logic
        // - Send welcome email
        // - Update search index
        // - Notify admin dashboard
        // - Trigger analytics event
        // - Update cache
        
        await Task.CompletedTask;
    }

    private async Task HandleStockUpdatedAsync(ProductStockUpdatedEvent @event)
    {
        _logger.LogInformation(
            "📦 Stock Updated: Product {ProductId} - {OldStock} → {NewStock} (Change: {Change})",
            @event.ProductId, @event.OldStock, @event.NewStock, @event.Change);

        // Check low stock warning
        if (@event.NewStock < 10)
        {
            _logger.LogWarning(
                "⚠️ LOW STOCK ALERT: Product {ProductId} has only {Stock} units remaining!",
                @event.ProductId, @event.NewStock);
            
            // TODO: Send notification to admin
            // TODO: Trigger reorder process
        }

        // Check out of stock
        if (@event.NewStock == 0)
        {
            _logger.LogError(
                "❌ OUT OF STOCK: Product {ProductId} is now out of stock!",
                @event.ProductId);
            
            // TODO: Send urgent notification
            // TODO: Update product status
        }

        await Task.CompletedTask;
    }

    private async Task HandlePriceChangedAsync(ProductPriceChangedEvent @event)
    {
        _logger.LogInformation(
            "💰 Price Changed: Product {ProductId} - {OldPrice:C} → {NewPrice:C}",
            @event.ProductId, @event.OldPrice, @event.NewPrice);

        // Calculate price change percentage
        var changePercent = ((@event.NewPrice - @event.OldPrice) / @event.OldPrice) * 100;
        
        if (Math.Abs(changePercent) > 20)
        {
            _logger.LogWarning(
                "⚠️ SIGNIFICANT PRICE CHANGE: Product {ProductId} changed by {Percent:F2}%",
                @event.ProductId, changePercent);
            
            // TODO: Notify customers who have this in wishlist
            // TODO: Update price history
        }

        if (@event.NewPrice < @event.OldPrice)
        {
            _logger.LogInformation(
                "🔽 Price Reduced: Product {ProductId} - Good deal! ({Percent:F2}% off)",
                @event.ProductId, Math.Abs(changePercent));
            
            // TODO: Send promotion notification
        }

        await Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Product Event Worker stopping at: {Time}", DateTimeOffset.Now);
        return base.StopAsync(cancellationToken);
    }
}