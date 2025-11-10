namespace ProductManagement.Domain.Exceptions;

public class InsufficientStockException : DomainException
{
    public InsufficientStockException(Guid productId, string productName, int requested, int available)
        : base($"Insufficient stock for product '{productName}'. Requested: {requested}, Available: {available}")
    {
        ProductId = productId;
        ProductName = productName;
        RequestedQuantity = requested;
        AvailableQuantity = available;
    }

    public Guid ProductId { get; }
    public string ProductName { get; }
    public int RequestedQuantity { get; }
    public int AvailableQuantity { get; }
}