namespace ProductManagement.Application.DTOs;

public record CreateWarehouseDto(
    string Code,
    string Name,
    string Address,
    string City,
    string State,
    string ZipCode,
    string? ContactPerson,
    string? Phone,
    int Priority = 99
);