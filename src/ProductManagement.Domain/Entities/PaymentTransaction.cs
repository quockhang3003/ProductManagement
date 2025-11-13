using ProductManagement.Domain.Enum;

namespace ProductManagement.Domain.Entities;

public class PaymentTransaction
{
    public Guid Id { get; private set; }
    public Guid PaymentId { get; private set; }
    public PaymentTransactionType Type { get; private set; }
    public decimal Amount { get; private set; }
    public bool IsSuccessful { get; private set; }
    public string? Notes { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Constructor for Dapper
    private PaymentTransaction() { }

    public PaymentTransaction(
        Guid paymentId,
        PaymentTransactionType type,
        decimal amount,
        bool isSuccessful,
        string? notes)
    {
        Id = Guid.NewGuid();
        PaymentId = paymentId;
        Type = type;
        Amount = amount;
        IsSuccessful = isSuccessful;
        Notes = notes;
        CreatedAt = DateTime.UtcNow;
    }
}