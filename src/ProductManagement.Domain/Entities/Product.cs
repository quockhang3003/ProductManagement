using ProductManagement.Domain.Events;

namespace ProductManagement.Domain.Entities;

public class Product
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public decimal Price { get; private set; }
    public int Stock { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public bool IsActive { get; private set; }

    // Constructor for Dapper
    private Product() 
    { 
        Name = string.Empty;
        Description = string.Empty;
    }

    public Product(string name, string description, decimal price, int stock)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? string.Empty;
        Price = price >= 0 ? price : throw new ArgumentException("Price must be positive");
        Stock = stock >= 0 ? stock : throw new ArgumentException("Stock must be positive");
        CreatedAt = DateTime.UtcNow;
        IsActive = true;

        // Raise domain event
        AddDomainEvent(new ProductCreatedEvent(Id, Name, Price, Stock));
    }

    public void UpdateDetails(string name, string description, decimal price)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? string.Empty;
        
        if (price != Price)
        {
            var oldPrice = Price;
            Price = price >= 0 ? price : throw new ArgumentException("Price must be positive");
            
            // Raise price changed event
            AddDomainEvent(new ProductPriceChangedEvent(Id, oldPrice, Price));
        }
        
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateStock(int quantity)
    {
        var oldStock = Stock;
        
        if (Stock + quantity < 0)
            throw new InvalidOperationException("Insufficient stock");
        
        Stock += quantity;
        UpdatedAt = DateTime.UtcNow;

        // Raise stock updated event
        AddDomainEvent(new ProductStockUpdatedEvent(Id, oldStock, Stock, quantity));
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ClearDomainEvents() => _domainEvents.Clear();

    private void AddDomainEvent(IDomainEvent eventItem) => _domainEvents.Add(eventItem);
}