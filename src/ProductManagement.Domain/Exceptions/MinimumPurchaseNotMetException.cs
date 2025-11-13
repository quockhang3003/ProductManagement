namespace ProductManagement.Domain.Exceptions;

public class MinimumPurchaseNotMetException : DomainException
{
    public MinimumPurchaseNotMetException(string code, decimal required, decimal actual)
        : base($"Promotion '{code}' requires minimum purchase of {required:C}, but order total is {actual:C}")
    {
        Code = code;
        RequiredAmount = required;
        ActualAmount = actual;
    }

    public string Code { get; }
    public decimal RequiredAmount { get; }
    public decimal ActualAmount { get; }
}