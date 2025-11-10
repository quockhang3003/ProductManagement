namespace ProductManagement.Domain.Exceptions;

public class InvalidOrderStateException : DomainException
{
    public InvalidOrderStateException(Guid orderId, string currentStatus, string attemptedAction)
        : base($"Cannot {attemptedAction} order {orderId} with status {currentStatus}")
    {
        OrderId = orderId;
        CurrentStatus = currentStatus;
        AttemptedAction = attemptedAction;
    }

    public Guid OrderId { get; }
    public string CurrentStatus { get; }
    public string AttemptedAction { get; }
}