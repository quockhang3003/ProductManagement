namespace ProductManagement.Domain.Exceptions;

public class PaymentGatewayNotConfiguredException : DomainException
{
    public PaymentGatewayNotConfiguredException(string paymentMethod)
        : base($"Payment gateway not configured for method: {paymentMethod}")
    {
        PaymentMethod = paymentMethod;
    }

    public string PaymentMethod { get; }
}