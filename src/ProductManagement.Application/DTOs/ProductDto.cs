namespace ProductManagement.Application.DTOs;

public record ProductDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int Stock,
    bool IsActive,
    DateTime CreatedAt
);