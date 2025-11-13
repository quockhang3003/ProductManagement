namespace ProductManagement.Domain.Enum;

public enum PaymentStatus
{
    Pending = 0,           // Initial state
    Authorized = 1,        // Payment authorized, not yet captured
    Captured = 2,          // Payment completed
    Failed = 3,            // Payment failed
    Expired = 4,           // Payment timeout
    Refunded = 5,          // Fully refunded
    PartiallyRefunded = 6  // Partially refunded

}