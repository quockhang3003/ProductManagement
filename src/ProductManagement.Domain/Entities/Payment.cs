using ProductManagement.Domain.Enum;
using ProductManagement.Domain.Events;

namespace ProductManagement.Domain.Entities;

public class Payment
{
    private readonly List<IDomainEvent> _domainEvents = new();
    private readonly List<PaymentTransaction> _transactions = new();
    
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public IReadOnlyCollection<PaymentTransaction> Transactions => _transactions.AsReadOnly();

    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; }
    public PaymentMethod Method { get; private set; }
    public PaymentStatus Status { get; private set; }
    public string? PaymentGateway { get; private set; }
    public string? TransactionId { get; private set; }
    public string? AuthorizationCode { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? AuthorizedAt { get; private set; }
    public DateTime? CapturedAt { get; private set; }
    public DateTime? FailedAt { get; private set; }
    public DateTime? RefundedAt { get; private set; }
    public string? FailureReason { get; private set; }
    public int RetryCount { get; private set; }
    public DateTime? ExpiresAt { get; private set; }

    // Installment info
    public bool IsInstallment { get; private set; }
    public int? InstallmentMonths { get; private set; }
    public decimal? InstallmentFee { get; private set; }

    // Constructor for Dapper
    private Payment()
    {
        Currency = "USD";
    }

    public Payment(
        Guid orderId,
        decimal amount,
        string currency,
        PaymentMethod method,
        bool isInstallment = false,
        int? installmentMonths = null)
    {
        if (amount <= 0)
            throw new ArgumentException("Payment amount must be positive");

        Id = Guid.NewGuid();
        OrderId = orderId;
        Amount = amount;
        Currency = currency ?? "USD";
        Method = method;
        Status = PaymentStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        ExpiresAt = DateTime.UtcNow.AddMinutes(15); // 15 min to complete
        RetryCount = 0;

        IsInstallment = isInstallment;
        InstallmentMonths = installmentMonths;
        if (isInstallment && installmentMonths.HasValue)
        {
            InstallmentFee = CalculateInstallmentFee(amount, installmentMonths.Value);
        }

        AddDomainEvent(new PaymentCreatedEvent(
            Id, OrderId, Amount, Currency, Method.ToString()));
    }

    public void Authorize(string transactionId, string authorizationCode, string gateway)
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException($"Cannot authorize payment with status {Status}");

        if (IsExpired())
            throw new InvalidOperationException("Payment has expired");

        Status = PaymentStatus.Authorized;
        TransactionId = transactionId;
        AuthorizationCode = authorizationCode;
        PaymentGateway = gateway;
        AuthorizedAt = DateTime.UtcNow;

        AddTransaction(PaymentTransactionType.Authorization, Amount, true, null);
        AddDomainEvent(new PaymentAuthorizedEvent(
            Id, OrderId, Amount, Method.ToString(), transactionId));
    }

    public void Capture()
    {
        if (Status != PaymentStatus.Authorized)
            throw new InvalidOperationException($"Cannot capture payment with status {Status}");

        Status = PaymentStatus.Captured;
        CapturedAt = DateTime.UtcNow;

        AddTransaction(PaymentTransactionType.Capture, Amount, true, null);
        AddDomainEvent(new PaymentCapturedEvent(
            Id, OrderId, Amount, Method.ToString(), TransactionId ?? ""));
    }

    public void Fail(string reason)
    {
        if (Status == PaymentStatus.Captured || Status == PaymentStatus.Refunded)
            throw new InvalidOperationException($"Cannot fail payment with status {Status}");

        var previousStatus = Status;
        Status = PaymentStatus.Failed;
        FailureReason = reason;
        FailedAt = DateTime.UtcNow;
        RetryCount++;

        AddTransaction(PaymentTransactionType.Failure, Amount, false, reason);
        AddDomainEvent(new PaymentFailedEvent(
            Id, OrderId, Amount, Method.ToString(), reason, RetryCount));
    }

    public void Retry()
    {
        if (Status != PaymentStatus.Failed)
            throw new InvalidOperationException("Can only retry failed payments");

        if (RetryCount >= 3)
            throw new InvalidOperationException("Maximum retry attempts reached");

        Status = PaymentStatus.Pending;
        FailureReason = null;
        FailedAt = null;
        ExpiresAt = DateTime.UtcNow.AddMinutes(15);

        AddDomainEvent(new PaymentRetryAttemptedEvent(Id, OrderId, RetryCount));
    }

    public void Refund(decimal amount, string reason)
    {
        if (Status != PaymentStatus.Captured)
            throw new InvalidOperationException("Can only refund captured payments");

        if (amount > Amount)
            throw new ArgumentException("Refund amount exceeds payment amount");

        var refundedAmount = _transactions
            .Where(t => t.Type == PaymentTransactionType.Refund && t.IsSuccessful)
            .Sum(t => t.Amount);

        if (refundedAmount + amount > Amount)
            throw new InvalidOperationException("Total refund exceeds payment amount");

        var isFullRefund = (refundedAmount + amount) >= Amount;

        if (isFullRefund)
        {
            Status = PaymentStatus.Refunded;
            RefundedAt = DateTime.UtcNow;
        }
        else
        {
            Status = PaymentStatus.PartiallyRefunded;
        }

        AddTransaction(PaymentTransactionType.Refund, amount, true, reason);
        AddDomainEvent(new PaymentRefundedEvent(
            Id, OrderId, amount, Amount, isFullRefund, reason));
    }

    public bool IsExpired()
    {
        return ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;
    }

    public bool CanRetry()
    {
        return Status == PaymentStatus.Failed && RetryCount < 3;
    }

    public decimal GetTotalRefunded()
    {
        return _transactions
            .Where(t => t.Type == PaymentTransactionType.Refund && t.IsSuccessful)
            .Sum(t => t.Amount);
    }

    public decimal GetTotalPaid()
    {
        if (Status == PaymentStatus.Captured || Status == PaymentStatus.PartiallyRefunded)
        {
            return Amount - GetTotalRefunded();
        }
        return 0;
    }

    private void AddTransaction(
        PaymentTransactionType type, 
        decimal amount, 
        bool isSuccessful, 
        string? notes)
    {
        var transaction = new PaymentTransaction(Id, type, amount, isSuccessful, notes);
        _transactions.Add(transaction);
    }

    private decimal CalculateInstallmentFee(decimal amount, int months)
    {
        // Simple fee calculation: 2% per month
        var feeRate = 0.02m * months;
        return amount * feeRate;
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
    private void AddDomainEvent(IDomainEvent eventItem) => _domainEvents.Add(eventItem);
}