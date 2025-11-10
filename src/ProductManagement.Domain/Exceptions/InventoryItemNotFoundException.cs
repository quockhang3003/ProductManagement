namespace ProductManagement.Domain.Exceptions;

public class InventoryItemNotFoundException : DomainException
{
    public InventoryItemNotFoundException(Guid inventoryItemId)
        : base($"Inventory item with ID {inventoryItemId} was not found")
    {
        InventoryItemId = inventoryItemId;
    }

    public InventoryItemNotFoundException(Guid productId, Guid warehouseId)
        : base($"Inventory item for Product {productId} in Warehouse {warehouseId} was not found")
    {
        ProductId = productId;
        WarehouseId = warehouseId;
    }

    public Guid? InventoryItemId { get; }
    public Guid? ProductId { get; }
    public Guid? WarehouseId { get; }
}