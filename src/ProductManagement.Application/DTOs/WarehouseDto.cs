namespace ProductManagement.Application.DTOs;

public record WarehouseDto(
    Guid Id,
    string Code,
    string Name,
    string Address,
    string City,
    string State,
    string ZipCode,
    string? ContactPerson,
    string? Phone,
    bool IsActive,
    int Priority,
    DateTime CreatedAt
);