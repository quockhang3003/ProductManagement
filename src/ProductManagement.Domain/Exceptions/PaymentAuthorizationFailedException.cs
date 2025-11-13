namespace ProductManagement.Domain.Exceptions;

public class PaymentAuthorizationFailedException : DomainException
{
    public PaymentAuthorizationFailedException(Guid paymentId, string reason)
        : base($"Payment {paymentId} authorization failed: {reason}")
    {
        PaymentId = paymentId;
        Reason = reason;
    }

    public Guid PaymentId { get; }
    public string Reason { get; }
}