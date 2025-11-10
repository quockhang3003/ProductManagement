using ProductManagement.Domain.Enum;
using ProductManagement.Domain.Events;

namespace ProductManagement.Domain.Entities;

public class StockTransfer
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid FromWarehouseId { get; private set; }
    public Guid ToWarehouseId { get; private set; }
    public int Quantity { get; private set; }
    public StockTransferStatus Status { get; private set; }
    public string? Notes { get; private set; }
    public string RequestedBy { get; private set; }
    public string? ApprovedBy { get; private set; }
    public DateTime RequestedAt { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }

    // Constructor for Dapper
    private StockTransfer()
    {
        RequestedBy = string.Empty;
    }

    public StockTransfer(
        Guid productId,
        Guid fromWarehouseId,
        Guid toWarehouseId,
        int quantity,
        string requestedBy,
        string? notes = null)
    {
        if (fromWarehouseId == toWarehouseId)
            throw new ArgumentException("Cannot transfer to the same warehouse");

        if (quantity <= 0)
            throw new ArgumentException("Transfer quantity must be positive");

        Id = Guid.NewGuid();
        ProductId = productId;
        FromWarehouseId = fromWarehouseId;
        ToWarehouseId = toWarehouseId;
        Quantity = quantity;
        Status = StockTransferStatus.Pending;
        Notes = notes;
        RequestedBy = requestedBy ?? throw new ArgumentNullException(nameof(requestedBy));
        RequestedAt = DateTime.UtcNow;

        AddDomainEvent(new StockTransferRequestedEvent(
            Id, ProductId, FromWarehouseId, ToWarehouseId, Quantity, RequestedBy));
    }

    public void Approve(string approvedBy)
    {
        if (Status != StockTransferStatus.Pending)
            throw new InvalidOperationException($"Cannot approve transfer with status {Status}");

        Status = StockTransferStatus.Approved;
        ApprovedBy = approvedBy;
        ApprovedAt = DateTime.UtcNow;

        AddDomainEvent(new StockTransferApprovedEvent(
            Id, ProductId, FromWarehouseId, ToWarehouseId, Quantity, ApprovedBy));
    }

    public void Complete()
    {
        if (Status != StockTransferStatus.Approved)
            throw new InvalidOperationException($"Cannot complete transfer with status {Status}");

        Status = StockTransferStatus.Completed;
        CompletedAt = DateTime.UtcNow;

        AddDomainEvent(new StockTransferCompletedEvent(
            Id, ProductId, FromWarehouseId, ToWarehouseId, Quantity));
    }

    public void Cancel(string reason)
    {
        if (Status == StockTransferStatus.Completed)
            throw new InvalidOperationException("Cannot cancel completed transfer");

        Status = StockTransferStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        CancellationReason = reason;

        AddDomainEvent(new StockTransferCancelledEvent(
            Id, ProductId, FromWarehouseId, ToWarehouseId, Quantity, reason));
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
    private void AddDomainEvent(IDomainEvent eventItem) => _domainEvents.Add(eventItem);
}