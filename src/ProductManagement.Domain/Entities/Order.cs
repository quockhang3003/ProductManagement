using ProductManagement.Domain.Events;

namespace ProductManagement.Domain.Entities;

public class Order
{
    private readonly List<IDomainEvent> _domainEvents = new();
    private readonly List<OrderItem> _items = new();

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public Guid Id { get; private set; }
    public string CustomerName { get; private set; }
    public string CustomerEmail { get; private set; }
    public string ShippingAddress { get; private set; }
    public string? PhoneNumber { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }
    public DateTime? ShippedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }
    public string? TrackingNumber { get; private set; }

    // Constructor for Dapper
    private Order()
    {
        CustomerName = string.Empty;
        CustomerEmail = string.Empty;
        ShippingAddress = string.Empty;
    }

    public Order(
        string customerName,
        string customerEmail,
        string shippingAddress,
        string? phoneNumber,
        List<OrderItem> items)
    {
        if (items == null || items.Count == 0)
            throw new ArgumentException("Order must have at least one item");

        Id = Guid.NewGuid();
        CustomerName = customerName ?? throw new ArgumentNullException(nameof(customerName));
        CustomerEmail = customerEmail ?? throw new ArgumentNullException(nameof(customerEmail));
        ShippingAddress = shippingAddress ?? throw new ArgumentNullException(nameof(shippingAddress));
        PhoneNumber = phoneNumber;
        Status = OrderStatus.Pending;
        CreatedAt = DateTime.UtcNow;

        _items.AddRange(items);
        CalculateTotalAmount();

        // Raise domain event
        AddDomainEvent(new OrderCreatedEvent(
            Id,
            CustomerName,
            CustomerEmail,
            TotalAmount,
            items.Select(i => new OrderItemData(i.ProductId, i.ProductName, i.Quantity, i.UnitPrice)).ToList()
        ));
    }

    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException($"Cannot confirm order with status {Status}");

        Status = OrderStatus.Confirmed;
        ConfirmedAt = DateTime.UtcNow;

        AddDomainEvent(new OrderConfirmedEvent(Id, CustomerName, CustomerEmail, TotalAmount));
    }

    public void Ship(string trackingNumber)
    {
        if (Status != OrderStatus.Confirmed)
            throw new InvalidOperationException($"Cannot ship order with status {Status}");

        if (string.IsNullOrWhiteSpace(trackingNumber))
            throw new ArgumentException("Tracking number is required");

        Status = OrderStatus.Shipping;
        ShippedAt = DateTime.UtcNow;
        TrackingNumber = trackingNumber;

        AddDomainEvent(new OrderShippedEvent(Id, CustomerName, CustomerEmail, TrackingNumber));
    }

    public void Deliver()
    {
        if (Status != OrderStatus.Shipping)
            throw new InvalidOperationException($"Cannot deliver order with status {Status}");

        Status = OrderStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;

        AddDomainEvent(new OrderDeliveredEvent(Id, CustomerName, CustomerEmail, TotalAmount));
    }

    public void Cancel(string reason)
    {
        if (Status == OrderStatus.Delivered)
            throw new InvalidOperationException("Cannot cancel delivered order");

        if (Status == OrderStatus.Cancelled)
            throw new InvalidOperationException("Order is already cancelled");

        var previousStatus = Status;
        Status = OrderStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        CancellationReason = reason ?? "No reason provided";

        AddDomainEvent(new OrderCancelledEvent(
            Id,
            CustomerName,
            CustomerEmail,
            previousStatus.ToString(),
            CancellationReason,
            _items.Select(i => new OrderItemData(i.ProductId, i.ProductName, i.Quantity, i.UnitPrice)).ToList()
        ));
    }

    private void CalculateTotalAmount()
    {
        TotalAmount = _items.Sum(item => item.SubTotal);
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
    private void AddDomainEvent(IDomainEvent eventItem) => _domainEvents.Add(eventItem);
}

public enum OrderStatus
{
    Pending = 0,      // Vừa tạo, chờ xác nhận
    Confirmed = 1,    // Đã xác nhận, chuẩn bị hàng
    Shipping = 2,     // Đang giao hàng
    Delivered = 3,    // Đã giao thành công
    Cancelled = 4     // Đã hủy
}

public class OrderItem
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal SubTotal => Quantity * UnitPrice;

    // Constructor for Dapper
    private OrderItem()
    {
        ProductName = string.Empty;
    }

    public OrderItem(Guid productId, string productName, int quantity, decimal unitPrice)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than 0");
        if (unitPrice < 0)
            throw new ArgumentException("Unit price must be non-negative");

        Id = Guid.NewGuid();
        ProductId = productId;
        ProductName = productName ?? throw new ArgumentNullException(nameof(productName));
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    internal void SetOrderId(Guid orderId)
    {
        OrderId = orderId;
    }
}