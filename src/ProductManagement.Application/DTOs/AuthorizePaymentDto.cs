namespace ProductManagement.Application.DTOs;

public record AuthorizePaymentDto(
    string PaymentDetails  // Card info, PayPal token, etc. (encrypted)
);