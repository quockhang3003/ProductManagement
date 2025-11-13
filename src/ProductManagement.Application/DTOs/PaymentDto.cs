namespace ProductManagement.Application.DTOs;

public record PaymentDto(
    Guid Id,
    Guid OrderId,
    decimal Amount,
    string Currency,
    string Method,
    string Status,
    string? PaymentGateway,
    string? TransactionId,
    string? AuthorizationCode,
    DateTime CreatedAt,
    DateTime? AuthorizedAt,
    DateTime? CapturedAt,
    DateTime? FailedAt,
    DateTime? RefundedAt,
    string? FailureReason,
    int RetryCount,
    DateTime? ExpiresAt,
    bool IsInstallment,
    int? InstallmentMonths,
    decimal? InstallmentFee,
    decimal TotalRefunded,
    decimal TotalPaid
);