namespace ProductManagement.Domain.Exceptions;

public class OrderNotFoundException : DomainException
{
    public OrderNotFoundException(Guid orderId) 
        : base($"Order with ID {orderId} was not found") 
    { 
        OrderId = orderId;
    }
    public Guid OrderId { get; }
}