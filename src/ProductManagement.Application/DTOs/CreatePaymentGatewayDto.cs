using ProductManagement.Domain.Enum;

namespace ProductManagement.Application.DTOs;

public record CreatePaymentGatewayDto(
    string Name,
    PaymentMethod SupportedMethod,
    string ApiKey,
    string ApiSecret,
    string WebhookUrl
);