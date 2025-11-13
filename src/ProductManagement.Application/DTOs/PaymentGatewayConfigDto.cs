namespace ProductManagement.Application.DTOs;

public record PaymentGatewayConfigDto(
    Guid Id,
    string Name,
    string SupportedMethod,
    bool IsActive,
    int TimeoutSeconds,
    decimal TransactionFeePercentage,
    decimal MinimumAmount,
    decimal MaximumAmount
);