namespace ProductManagement.Domain.Exceptions;

public class PaymentExpiredException : DomainException
{
    public PaymentExpiredException(Guid paymentId)
        : base($"Payment {paymentId} has expired")
    {
        PaymentId = paymentId;
    }

    public Guid PaymentId { get; }
}