namespace ProductManagement.Application.DTOs;

public record AdjustStockDto(
    int Adjustment,  // Positive or negative
    string Reason,
    string UserId
);