namespace ProductManagement.Domain.Exceptions;

public class WarehouseNotFoundException : DomainException
{
    public WarehouseNotFoundException(Guid warehouseId)
        : base($"Warehouse with ID {warehouseId} was not found")
    {
        WarehouseId = warehouseId;
    }

    public Guid WarehouseId { get; }
}