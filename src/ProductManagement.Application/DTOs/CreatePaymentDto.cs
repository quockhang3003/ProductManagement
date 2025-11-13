using ProductManagement.Domain.Enum;

namespace ProductManagement.Application.DTOs;

public record CreatePaymentDto(
    Guid OrderId,
    decimal Amount,
    string Currency,
    PaymentMethod Method,
    bool IsInstallment = false,
    int? InstallmentMonths = null
);
