namespace ProductManagement.Application.DTOs;

public record RefundPaymentDto(
    decimal Amount,
    string Reason
);