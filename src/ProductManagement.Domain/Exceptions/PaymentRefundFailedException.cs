namespace ProductManagement.Domain.Exceptions;

public class PaymentRefundFailedException : DomainException
{
    public PaymentRefundFailedException(Guid paymentId, string reason)
        : base($"Payment {paymentId} refund failed: {reason}")
    {
        PaymentId = paymentId;
        Reason = reason;
    }

    public Guid PaymentId { get; }
    public string Reason { get; }
}