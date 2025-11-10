using ProductManagement.Domain.Events;

namespace ProductManagement.Domain.Entities;

public class Warehouse
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public Guid Id { get; private set; }
    public string Code { get; private set; }  // WHN, WHE, WHW, etc.
    public string Name { get; private set; }
    public string Address { get; private set; }
    public string City { get; private set; }
    public string State { get; private set; }
    public string ZipCode { get; private set; }
    public string? ContactPerson { get; private set; }
    public string? Phone { get; private set; }
    public bool IsActive { get; private set; }
    public int Priority { get; private set; }  // 1 = highest priority
    public DateTime CreatedAt { get; private set; }

    // Constructor for Dapper
    private Warehouse()
    {
        Code = string.Empty;
        Name = string.Empty;
        Address = string.Empty;
        City = string.Empty;
        State = string.Empty;
        ZipCode = string.Empty;
    }

    public Warehouse(
        string code, 
        string name, 
        string address, 
        string city, 
        string state, 
        string zipCode,
        string? contactPerson,
        string? phone,
        int priority = 99)
    {
        Id = Guid.NewGuid();
        Code = code ?? throw new ArgumentNullException(nameof(code));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Address = address ?? throw new ArgumentNullException(nameof(address));
        City = city ?? throw new ArgumentNullException(nameof(city));
        State = state ?? throw new ArgumentNullException(nameof(state));
        ZipCode = zipCode ?? throw new ArgumentNullException(nameof(zipCode));
        ContactPerson = contactPerson;
        Phone = phone;
        IsActive = true;
        Priority = priority;
        CreatedAt = DateTime.UtcNow;

        AddDomainEvent(new WarehouseCreatedEvent(Id, Code, Name, City, State));
    }

    public void Deactivate()
    {
        IsActive = false;
        AddDomainEvent(new WarehouseDeactivatedEvent(Id, Code, Name));
    }

    public void Activate()
    {
        IsActive = true;
        AddDomainEvent(new WarehouseActivatedEvent(Id, Code, Name));
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
    private void AddDomainEvent(IDomainEvent eventItem) => _domainEvents.Add(eventItem);
}