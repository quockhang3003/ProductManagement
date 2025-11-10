using ProductManagement.Application.DTOs;

namespace ProductManagement.Application.Services;

public interface IProductService
{
    Task<ProductDto?> GetProductByIdAsync(Guid id);
    Task<IEnumerable<ProductDto>> GetAllProductsAsync();
    Task<IEnumerable<ProductDto>> GetActiveProductsAsync();
    Task<ProductDto> CreateProductAsync(CreateProductDto dto);
    Task UpdateProductAsync(UpdateProductDto dto);
    Task UpdateStockAsync(Guid id, int quantity);
    Task DeleteProductAsync(Guid id);
    Task ActivateProductAsync(Guid id);
    Task DeactivateProductAsync(Guid id);
}