namespace ProductManagement.Domain.Exceptions;

public class StockTransferNotFoundException : DomainException
{
    public StockTransferNotFoundException(Guid transferId)
        : base($"Stock transfer with ID {transferId} was not found")
    {
        TransferId = transferId;
    }

    public Guid TransferId { get; }
}