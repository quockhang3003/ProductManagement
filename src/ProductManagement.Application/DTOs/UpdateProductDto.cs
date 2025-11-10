namespace ProductManagement.Application.DTOs;

public record UpdateProductDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price
);