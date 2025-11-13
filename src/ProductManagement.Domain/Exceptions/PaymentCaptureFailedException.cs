namespace ProductManagement.Domain.Exceptions;

public class PaymentCaptureFailedException : DomainException
{
    public PaymentCaptureFailedException(Guid paymentId, string reason)
        : base($"Payment {paymentId} capture failed: {reason}")
    {
        PaymentId = paymentId;
        Reason = reason;
    }

    public Guid PaymentId { get; }
    public string Reason { get; }
}