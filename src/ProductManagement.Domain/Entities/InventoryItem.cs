using ProductManagement.Domain.Events;

namespace ProductManagement.Domain.Entities;

public class InventoryItem
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public Guid Id { get; private set; }
    public Guid WarehouseId { get; private set; }
    public Guid ProductId { get; private set; }
    public int QuantityOnHand { get; private set; }      // Số lượng thực tế
    public int QuantityReserved { get; private set; }    // Đã giữ cho đơn hàng
    public int QuantityAvailable => QuantityOnHand - QuantityReserved;  // Có thể bán
    public int ReorderPoint { get; private set; }        // Mức cảnh báo cần order
    public int ReorderQuantity { get; private set; }     // Số lượng order khi hết
    public DateTime LastRestockedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Constructor for Dapper
    private InventoryItem() { }

    public InventoryItem(
        Guid warehouseId, 
        Guid productId, 
        int initialQuantity,
        int reorderPoint = 10,
        int reorderQuantity = 50)
    {
        if (initialQuantity < 0)
            throw new ArgumentException("Initial quantity cannot be negative");

        Id = Guid.NewGuid();
        WarehouseId = warehouseId;
        ProductId = productId;
        QuantityOnHand = initialQuantity;
        QuantityReserved = 0;
        ReorderPoint = reorderPoint;
        ReorderQuantity = reorderQuantity;
        LastRestockedAt = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;

        AddDomainEvent(new InventoryItemCreatedEvent(
            Id, WarehouseId, ProductId, initialQuantity));
    }

    public void AdjustStock(int quantity, string reason, string userId)
    {
        var oldQuantity = QuantityOnHand;
        QuantityOnHand += quantity;

        if (QuantityOnHand < 0)
            throw new InvalidOperationException("Stock cannot be negative");

        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new StockAdjustedEvent(
            Id, WarehouseId, ProductId, oldQuantity, QuantityOnHand, quantity, reason, userId));

        // Check if reorder needed
        if (QuantityAvailable <= ReorderPoint)
        {
            AddDomainEvent(new LowStockAlertEvent(
                ProductId, WarehouseId, QuantityAvailable, ReorderPoint, ReorderQuantity));
        }
    }

    public void Restock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Restock quantity must be positive");

        var oldQuantity = QuantityOnHand;
        QuantityOnHand += quantity;
        LastRestockedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new StockRestockedEvent(
            Id, WarehouseId, ProductId, oldQuantity, QuantityOnHand, quantity));
    }

    public void Reserve(int quantity, Guid orderId)
    {
        if (quantity <= 0)
            throw new ArgumentException("Reserve quantity must be positive");

        if (QuantityAvailable < quantity)
            throw new InvalidOperationException(
                $"Cannot reserve {quantity} units. Only {QuantityAvailable} available");

        QuantityReserved += quantity;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new StockReservedEvent(
            Id, WarehouseId, ProductId, quantity, QuantityReserved, orderId));
    }

    public void ReleaseReservation(int quantity, Guid orderId)
    {
        if (quantity <= 0)
            throw new ArgumentException("Release quantity must be positive");

        if (QuantityReserved < quantity)
            throw new InvalidOperationException(
                $"Cannot release {quantity} units. Only {QuantityReserved} reserved");

        QuantityReserved -= quantity;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new StockReservationReleasedEvent(
            Id, WarehouseId, ProductId, quantity, QuantityReserved, orderId));
    }

    public void FulfillReservation(int quantity, Guid orderId)
    {
        if (quantity <= 0)
            throw new ArgumentException("Fulfill quantity must be positive");

        if (QuantityReserved < quantity)
            throw new InvalidOperationException("Not enough reserved stock");

        if (QuantityOnHand < quantity)
            throw new InvalidOperationException("Not enough stock on hand");

        QuantityReserved -= quantity;
        QuantityOnHand -= quantity;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new StockFulfilledEvent(
            Id, WarehouseId, ProductId, quantity, QuantityOnHand, orderId));
    }

    public void UpdateReorderSettings(int reorderPoint, int reorderQuantity)
    {
        ReorderPoint = reorderPoint;
        ReorderQuantity = reorderQuantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
    private void AddDomainEvent(IDomainEvent eventItem) => _domainEvents.Add(eventItem);
}
