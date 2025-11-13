namespace ProductManagement.Domain.Exceptions;

public class PaymentNotFoundException : DomainException
{
    public PaymentNotFoundException(Guid paymentId)
        : base($"Payment with ID {paymentId} was not found")
    {
        PaymentId = paymentId;
    }

    public Guid PaymentId { get; }
}