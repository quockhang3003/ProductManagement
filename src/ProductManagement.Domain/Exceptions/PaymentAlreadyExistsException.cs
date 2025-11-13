namespace ProductManagement.Domain.Exceptions;

public class PaymentAlreadyExistsException : DomainException
{
    public PaymentAlreadyExistsException(Guid orderId)
        : base($"Payment already exists for Order {orderId}")
    {
        OrderId = orderId;
    }

    public Guid OrderId { get; }
}