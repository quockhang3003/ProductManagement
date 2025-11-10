using ProductManagement.Application.DTOs;
using ProductManagement.Application.Messaging;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Exceptions;
using ProductManagement.Domain.Repositories;

namespace ProductManagement.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly IMessagePublisher _messagePublisher;

    public ProductService(IProductRepository repository, IMessagePublisher messagePublisher)
    {
        _repository = repository;
        _messagePublisher = messagePublisher;
    }

    public async Task<ProductDto?> GetProductByIdAsync(Guid id)
    {
        var product = await _repository.GetByIdAsync(id);
        return product == null ? null : MapToDto(product);
    }

    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
    {
        var products = await _repository.GetAllAsync();
        return products.Select(MapToDto);
    }

    public async Task<IEnumerable<ProductDto>> GetActiveProductsAsync()
    {
        var products = await _repository.GetActiveProductsAsync();
        return products.Select(MapToDto);
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
    {
        var product = new Product(dto.Name, dto.Description, dto.Price, dto.Stock);
        await _repository.AddAsync(product);

        // Publish domain events to RabbitMQ
        await PublishDomainEvents(product);

        return MapToDto(product);
    }

    public async Task UpdateProductAsync(UpdateProductDto dto)
    {
        var product = await _repository.GetByIdAsync(dto.Id);
        if (product == null)
            throw new ProductNotFoundException(dto.Id);

        product.UpdateDetails(dto.Name, dto.Description, dto.Price);
        await _repository.UpdateAsync(product);

        // Publish domain events
        await PublishDomainEvents(product);
    }

    public async Task UpdateStockAsync(Guid id, int quantity)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product == null)
            throw new ProductNotFoundException(id);

        product.UpdateStock(quantity);
        await _repository.UpdateAsync(product);

        // Publish domain events
        await PublishDomainEvents(product);
    }

    public async Task DeleteProductAsync(Guid id)
    {
        if (!await _repository.ExistsAsync(id))
            throw new ProductNotFoundException(id);

        await _repository.DeleteAsync(id);
    }

    public async Task ActivateProductAsync(Guid id)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product == null)
            throw new ProductNotFoundException(id);

        product.Activate();
        await _repository.UpdateAsync(product);
    }

    public async Task DeactivateProductAsync(Guid id)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product == null)
            throw new ProductNotFoundException(id);

        product.Deactivate();
        await _repository.UpdateAsync(product);
    }

    private async Task PublishDomainEvents(Product product)
    {
        foreach (var domainEvent in product.DomainEvents)
        {
            await _messagePublisher.PublishAsync(domainEvent, "product-events");
        }
        product.ClearDomainEvents();
    }

    private static ProductDto MapToDto(Product product) => new(
        product.Id,
        product.Name,
        product.Description,
        product.Price,
        product.Stock,
        product.IsActive,
        product.CreatedAt
    );
}