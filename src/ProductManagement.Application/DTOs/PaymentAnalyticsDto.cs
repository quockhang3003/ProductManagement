namespace ProductManagement.Application.DTOs;

public record PaymentAnalyticsDto(
    int TotalPayments,
    int SuccessfulPayments,
    int FailedPayments,
    decimal TotalRevenue,
    decimal TotalRefunded,
    decimal SuccessRate,
    Dictionary<string, int> PaymentsByMethod,
    DateTime FromDate,
    DateTime ToDate
);