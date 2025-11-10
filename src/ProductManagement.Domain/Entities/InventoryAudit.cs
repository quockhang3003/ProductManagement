namespace ProductManagement.Domain.Entities;

public class InventoryAudit
{
    public Guid Id { get; private set; }
    public Guid WarehouseId { get; private set; }
    public Guid ProductId { get; private set; }
    public int ExpectedQuantity { get; private set; }
    public int ActualQuantity { get; private set; }
    public int Variance => ActualQuantity - ExpectedQuantity;
    public string AuditedBy { get; private set; }
    public string? Notes { get; private set; }
    public DateTime AuditedAt { get; private set; }

    // Constructor for Dapper
    private InventoryAudit()
    {
        AuditedBy = string.Empty;
    }

    public InventoryAudit(
        Guid warehouseId,
        Guid productId,
        int expectedQuantity,
        int actualQuantity,
        string auditedBy,
        string? notes = null)
    {
        Id = Guid.NewGuid();
        WarehouseId = warehouseId;
        ProductId = productId;
        ExpectedQuantity = expectedQuantity;
        ActualQuantity = actualQuantity;
        AuditedBy = auditedBy ?? throw new ArgumentNullException(nameof(auditedBy));
        Notes = notes;
        AuditedAt = DateTime.UtcNow;
    }
}