namespace ProductManagement.Domain.Enum;

public enum PaymentTransactionType
{
    Authorization = 0,
    Capture = 1,
    Refund = 2,
    Failure = 3
}