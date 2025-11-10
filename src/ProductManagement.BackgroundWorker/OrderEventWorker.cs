using ProductManagement.Application.Messaging;
using ProductManagement.Domain.Events;

namespace ProductManagement.BackgroundWorker;

public class OrderEventWorker : BackgroundService
{
    private readonly IMessageConsumer _messageConsumer;
    private readonly ILogger<OrderEventWorker> _logger;

    public OrderEventWorker(
        IMessageConsumer messageConsumer,
        ILogger<OrderEventWorker> logger)
    {
        _messageConsumer = messageConsumer;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Order Event Worker starting at: {Time}", DateTimeOffset.Now);

        // Subscribe to all order events
        _messageConsumer.Subscribe<OrderCreatedEvent>(
            "order-events",
            HandleOrderCreatedAsync);

        _messageConsumer.Subscribe<OrderConfirmedEvent>(
            "order-events",
            HandleOrderConfirmedAsync);

        _messageConsumer.Subscribe<OrderShippedEvent>(
            "order-events",
            HandleOrderShippedAsync);

        _messageConsumer.Subscribe<OrderDeliveredEvent>(
            "order-events",
            HandleOrderDeliveredAsync);

        _messageConsumer.Subscribe<OrderCancelledEvent>(
            "order-events",
            HandleOrderCancelledAsync);

        _messageConsumer.StartConsuming();

        _logger.LogInformation("Order Event Worker is now listening to events...");

        return Task.CompletedTask;
    }

    private async Task HandleOrderCreatedAsync(OrderCreatedEvent @event)
    {
        _logger.LogInformation(
            "🛒 ORDER CREATED: {OrderId} | Customer: {Customer} | Total: {Total:C} | Items: {ItemCount}",
            @event.OrderId, @event.CustomerName, @event.TotalAmount, @event.Items.Count);

        // Log items
        foreach (var item in @event.Items)
        {
            _logger.LogInformation(
                "   📦 {ProductName} x{Quantity} @ {Price:C} = {SubTotal:C}",
                item.ProductName, item.Quantity, item.UnitPrice, 
                item.Quantity * item.UnitPrice);
        }

        // TODO: Business logic
        // ✅ Send order confirmation email to customer
        _logger.LogInformation("   ✉️  TODO: Send confirmation email to {Email}", @event.CustomerEmail);
        
        // ✅ Notify warehouse to prepare items
        _logger.LogInformation("   📋 TODO: Notify warehouse for order {OrderId}", @event.OrderId);
        
        // ✅ Create invoice
        _logger.LogInformation("   🧾 TODO: Generate invoice for order {OrderId}", @event.OrderId);
        
        // ✅ Trigger analytics event
        _logger.LogInformation("   📊 TODO: Log analytics - New order worth {Total:C}", @event.TotalAmount);

        await Task.CompletedTask;
    }

    private async Task HandleOrderConfirmedAsync(OrderConfirmedEvent @event)
    {
        _logger.LogInformation(
            "✅ ORDER CONFIRMED: {OrderId} | Customer: {Customer} | Total: {Total:C}",
            @event.OrderId, @event.CustomerName, @event.TotalAmount);

        // TODO: Business logic
        // ✅ Send confirmation email
        _logger.LogInformation("   ✉️  TODO: Send 'Order Confirmed' email to {Email}", @event.CustomerEmail);
        
        // ✅ Start preparation process
        _logger.LogInformation("   🏭 TODO: Start order preparation for {OrderId}", @event.OrderId);
        
        // ✅ Schedule payment processing
        _logger.LogInformation("   💳 TODO: Process payment for {Total:C}", @event.TotalAmount);

        await Task.CompletedTask;
    }

    private async Task HandleOrderShippedAsync(OrderShippedEvent @event)
    {
        _logger.LogInformation(
            "🚚 ORDER SHIPPED: {OrderId} | Tracking: {TrackingNumber} | Customer: {Customer}",
            @event.OrderId, @event.TrackingNumber, @event.CustomerName);

        // TODO: Business logic
        // ✅ Send shipping notification with tracking
        _logger.LogInformation(
            "   ✉️  TODO: Send tracking email to {Email} with tracking: {Tracking}",
            @event.CustomerEmail, @event.TrackingNumber);
        
        // ✅ Update order status on website
        _logger.LogInformation("   🌐 TODO: Update order status in customer portal");
        
        // ✅ Send SMS notification
        _logger.LogInformation("   📱 TODO: Send SMS with tracking number");
        
        // ✅ Schedule delivery reminder
        _logger.LogInformation("   ⏰ TODO: Schedule delivery reminder for 3 days later");

        await Task.CompletedTask;
    }

    private async Task HandleOrderDeliveredAsync(OrderDeliveredEvent @event)
    {
        _logger.LogInformation(
            "📦 ORDER DELIVERED: {OrderId} | Customer: {Customer} | Total: {Total:C}",
            @event.OrderId, @event.CustomerName, @event.TotalAmount);

        // TODO: Business logic
        // ✅ Send delivery confirmation
        _logger.LogInformation("   ✉️  TODO: Send delivery confirmation to {Email}", @event.CustomerEmail);
        
        // ✅ Request product review
        _logger.LogInformation("   ⭐ TODO: Request product review from customer");
        
        // ✅ Update revenue statistics
        _logger.LogInformation("   💰 TODO: Add {Total:C} to revenue statistics", @event.TotalAmount);
        
        // ✅ Trigger loyalty points
        _logger.LogInformation("   🎁 TODO: Award loyalty points for {Total:C} purchase", @event.TotalAmount);
        
        // ✅ Close support tickets
        _logger.LogInformation("   🎫 TODO: Auto-close support tickets for order {OrderId}", @event.OrderId);

        await Task.CompletedTask;
    }

    private async Task HandleOrderCancelledAsync(OrderCancelledEvent @event)
    {
        _logger.LogWarning(
            "❌ ORDER CANCELLED: {OrderId} | Status was: {PreviousStatus} | Reason: {Reason}",
            @event.OrderId, @event.PreviousStatus, @event.CancellationReason);

        _logger.LogInformation(
            "   Customer: {Customer} | Email: {Email}",
            @event.CustomerName, @event.CustomerEmail);

        // Log returned items
        _logger.LogInformation("   📦 Returning stock for {ItemCount} items:", @event.Items.Count);
        foreach (var item in @event.Items)
        {
            _logger.LogInformation(
                "      ↩️  {ProductName} x{Quantity}",
                item.ProductName, item.Quantity);
        }

        // TODO: Business logic
        // ✅ Send cancellation confirmation
        _logger.LogInformation("   ✉️  TODO: Send cancellation email to {Email}", @event.CustomerEmail);
        
        // ✅ Process refund (if payment was made)
        if (@event.PreviousStatus == "Confirmed" || @event.PreviousStatus == "Shipping")
        {
            _logger.LogInformation("   💸 TODO: Process refund for order {OrderId}", @event.OrderId);
        }
        
        // ✅ Cancel shipment (if in shipping)
        if (@event.PreviousStatus == "Shipping")
        {
            _logger.LogWarning("   🚫 TODO: Cancel shipment for order {OrderId}", @event.OrderId);
        }
        
        // ✅ Update analytics
        _logger.LogInformation("   📊 TODO: Log cancellation reason in analytics");
        
        // ✅ Investigate if pattern of cancellations
        _logger.LogInformation("   🔍 TODO: Check if customer has multiple cancellations");

        await Task.CompletedTask;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Order Event Worker stopping at: {Time}", DateTimeOffset.Now);
        return base.StopAsync(cancellationToken);
    }
}