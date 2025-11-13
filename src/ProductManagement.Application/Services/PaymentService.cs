using Microsoft.Extensions.Logging;
using ProductManagement.Application.DTOs;
using ProductManagement.Application.Messaging;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Enum;
using ProductManagement.Domain.Exceptions;
using ProductManagement.Domain.Repositories;

namespace ProductManagement.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepo;
    private readonly IPaymentGatewayRepository _gatewayRepo;
    private readonly IOrderRepository _orderRepo;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(
        IPaymentRepository paymentRepo,
        IPaymentGatewayRepository gatewayRepo,
        IOrderRepository orderRepo,
        IMessagePublisher messagePublisher,
        ILogger<PaymentService> logger)
    {
        _paymentRepo = paymentRepo;
        _gatewayRepo = gatewayRepo;
        _orderRepo = orderRepo;
        _messagePublisher = messagePublisher;
        _logger = logger;
    }

    public async Task<PaymentDto> CreatePaymentAsync(CreatePaymentDto dto)
    {
        // Validate order exists
        var order = await _orderRepo.GetByIdAsync(dto.OrderId);
        if (order == null)
            throw new OrderNotFoundException(dto.OrderId);

        // Check if payment already exists for this order
        var existingPayment = await _paymentRepo.GetByOrderIdAsync(dto.OrderId);
        if (existingPayment != null && existingPayment.Status != PaymentStatus.Failed)
            throw new InvalidOperationException($"Payment already exists for order {dto.OrderId}");

        // Validate payment method is supported
        var gateway = await _gatewayRepo.GetByPaymentMethodAsync(dto.Method);
        if (gateway == null || !gateway.IsActive)
            throw new InvalidOperationException($"Payment method {dto.Method} is not supported");

        // Validate amount
        if (dto.Amount < gateway.MinimumAmount || dto.Amount > gateway.MaximumAmount)
            throw new InvalidOperationException(
                $"Amount must be between {gateway.MinimumAmount} and {gateway.MaximumAmount}");

        var payment = new Payment(
            dto.OrderId,
            dto.Amount,
            dto.Currency,
            dto.Method,
            dto.IsInstallment,
            dto.InstallmentMonths);

        await _paymentRepo.AddAsync(payment);
        await PublishDomainEvents(payment);

        _logger.LogInformation(
            "Payment {PaymentId} created for Order {OrderId}, Amount: {Amount} {Currency}",
            payment.Id, dto.OrderId, dto.Amount, dto.Currency);

        return MapToDto(payment);
    }

    public async Task<PaymentDto> AuthorizePaymentAsync(Guid paymentId, AuthorizePaymentDto dto)
    {
        var payment = await _paymentRepo.GetByIdAsync(paymentId);
        if (payment == null)
            throw new PaymentNotFoundException(paymentId);

        // Simulate gateway authorization (in real app, call actual gateway API)
        var gateway = await _gatewayRepo.GetByPaymentMethodAsync(payment.Method);
        if (gateway == null)
            throw new InvalidOperationException("Payment gateway not configured");

        try
        {
            // TODO: Call real payment gateway API
            // var result = await gateway.AuthorizeAsync(payment, dto.PaymentDetails);
            
            // Simulate authorization
            var transactionId = $"TXN-{Guid.NewGuid().ToString()[..8].ToUpper()}";
            var authCode = $"AUTH-{Guid.NewGuid().ToString()[..6].ToUpper()}";

            payment.Authorize(transactionId, authCode, gateway.Name);
            
            await _paymentRepo.UpdateAsync(payment);
            await PublishDomainEvents(payment);

            _logger.LogInformation(
                "Payment {PaymentId} authorized: {TransactionId}",
                paymentId, transactionId);

            return MapToDto(payment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authorization failed for Payment {PaymentId}", paymentId);
            
            payment.Fail($"Authorization failed: {ex.Message}");
            await _paymentRepo.UpdateAsync(payment);
            await PublishDomainEvents(payment);

            throw;
        }
    }

    public async Task<PaymentDto> CapturePaymentAsync(Guid paymentId)
    {
        var payment = await _paymentRepo.GetByIdAsync(paymentId);
        if (payment == null)
            throw new PaymentNotFoundException(paymentId);

        if (payment.Status != PaymentStatus.Authorized)
            throw new InvalidOperationException($"Cannot capture payment with status {payment.Status}");

        try
        {
            // TODO: Call real payment gateway capture API
            // var result = await gateway.CaptureAsync(payment.TransactionId);

            payment.Capture();
            
            await _paymentRepo.UpdateAsync(payment);
            await PublishDomainEvents(payment);

            _logger.LogInformation(
                "Payment {PaymentId} captured successfully",
                paymentId);

            return MapToDto(payment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Capture failed for Payment {PaymentId}", paymentId);
            
            payment.Fail($"Capture failed: {ex.Message}");
            await _paymentRepo.UpdateAsync(payment);
            await PublishDomainEvents(payment);

            throw;
        }
    }

    public async Task RetryFailedPaymentAsync(Guid paymentId)
    {
        var payment = await _paymentRepo.GetByIdAsync(paymentId);
        if (payment == null)
            throw new PaymentNotFoundException(paymentId);

        if (!payment.CanRetry())
            throw new InvalidOperationException("Payment cannot be retried");

        payment.Retry();
        
        await _paymentRepo.UpdateAsync(payment);
        await PublishDomainEvents(payment);

        _logger.LogInformation(
            "Payment {PaymentId} retry initiated (Attempt {RetryCount})",
            paymentId, payment.RetryCount);
    }

    public async Task<PaymentDto> RefundPaymentAsync(Guid paymentId, RefundPaymentDto dto)
    {
        var payment = await _paymentRepo.GetByIdAsync(paymentId);
        if (payment == null)
            throw new PaymentNotFoundException(paymentId);

        if (payment.Status != PaymentStatus.Captured && 
            payment.Status != PaymentStatus.PartiallyRefunded)
            throw new InvalidOperationException($"Cannot refund payment with status {payment.Status}");

        try
        {
            // TODO: Call real payment gateway refund API
            // var result = await gateway.RefundAsync(payment.TransactionId, dto.Amount);

            payment.Refund(dto.Amount, dto.Reason);
            
            await _paymentRepo.UpdateAsync(payment);
            await PublishDomainEvents(payment);

            _logger.LogInformation(
                "Payment {PaymentId} refunded: {Amount} (Reason: {Reason})",
                paymentId, dto.Amount, dto.Reason);

            return MapToDto(payment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Refund failed for Payment {PaymentId}", paymentId);
            throw;
        }
    }

    public async Task<PaymentDto?> GetPaymentByIdAsync(Guid id)
    {
        var payment = await _paymentRepo.GetByIdAsync(id);
        return payment == null ? null : MapToDto(payment);
    }

    public async Task<PaymentDto?> GetPaymentByOrderIdAsync(Guid orderId)
    {
        var payment = await _paymentRepo.GetByOrderIdAsync(orderId);
        return payment == null ? null : MapToDto(payment);
    }

    public async Task<IEnumerable<PaymentDto>> GetPaymentsByStatusAsync(string status)
    {
        if (!Enum.TryParse<PaymentStatus>(status, true, out var paymentStatus))
            throw new ArgumentException($"Invalid payment status: {status}");

        var payments = await _paymentRepo.GetByStatusAsync(paymentStatus);
        return payments.Select(MapToDto);
    }

    public async Task<IEnumerable<PaymentDto>> GetFailedPaymentsAsync()
    {
        var payments = await _paymentRepo.GetFailedPaymentsAsync();
        return payments.Select(MapToDto);
    }

    public async Task<IEnumerable<PaymentDto>> GetExpiredPaymentsAsync()
    {
        var payments = await _paymentRepo.GetExpiredPaymentsAsync();
        return payments.Select(MapToDto);
    }

    public async Task<PaymentAnalyticsDto> GetPaymentAnalyticsAsync(DateTime from, DateTime to)
    {
        var allPayments = await _paymentRepo.GetPaymentsBetweenDatesAsync(from, to);

        var totalPayments = allPayments.Count();
        var successfulPayments = allPayments.Count(p => p.Status == PaymentStatus.Captured);
        var failedPayments = allPayments.Count(p => p.Status == PaymentStatus.Failed);
        var totalRevenue = allPayments
            .Where(p => p.Status == PaymentStatus.Captured)
            .Sum(p => p.GetTotalPaid());
        var totalRefunded = allPayments
            .Sum(p => p.GetTotalRefunded());

        var paymentsByMethod = allPayments
            .GroupBy(p => p.Method)
            .ToDictionary(g => g.Key.ToString(), g => g.Count());

        var successRate = totalPayments > 0 
            ? (decimal)successfulPayments / totalPayments * 100 
            : 0;

        return new PaymentAnalyticsDto(
            totalPayments,
            successfulPayments,
            failedPayments,
            totalRevenue,
            totalRefunded,
            successRate,
            paymentsByMethod,
            from,
            to
        );
    }

    public async Task<decimal> GetTotalRevenueAsync(DateTime from, DateTime to)
    {
        return await _paymentRepo.GetTotalRevenueAsync(from, to);
    }

    public async Task<PaymentGatewayConfigDto> AddPaymentGatewayAsync(CreatePaymentGatewayDto dto)
    {
        var gateway = new PaymentGatewayConfig(
            dto.Name,
            dto.SupportedMethod,
            dto.ApiKey,
            dto.ApiSecret,
            dto.WebhookUrl);

        await _gatewayRepo.AddAsync(gateway);

        _logger.LogInformation(
            "Payment gateway added: {Name} for {Method}",
            dto.Name, dto.SupportedMethod);

        return MapToDto(gateway);
    }

    public async Task<IEnumerable<PaymentGatewayConfigDto>> GetActivePaymentGatewaysAsync()
    {
        var gateways = await _gatewayRepo.GetActiveGatewaysAsync();
        return gateways.Select(MapToDto);
    }

    private async Task PublishDomainEvents(Payment payment)
    {
        foreach (var domainEvent in payment.DomainEvents)
        {
            await _messagePublisher.PublishAsync(domainEvent, "payment-events");
        }
        payment.ClearDomainEvents();
    }

    private static PaymentDto MapToDto(Payment payment) => new(
        payment.Id,
        payment.OrderId,
        payment.Amount,
        payment.Currency,
        payment.Method.ToString(),
        payment.Status.ToString(),
        payment.PaymentGateway,
        payment.TransactionId,
        payment.AuthorizationCode,
        payment.CreatedAt,
        payment.AuthorizedAt,
        payment.CapturedAt,
        payment.FailedAt,
        payment.RefundedAt,
        payment.FailureReason,
        payment.RetryCount,
        payment.ExpiresAt,
        payment.IsInstallment,
        payment.InstallmentMonths,
        payment.InstallmentFee,
        payment.GetTotalRefunded(),
        payment.GetTotalPaid()
    );

    private static PaymentGatewayConfigDto MapToDto(PaymentGatewayConfig gateway) => new(
        gateway.Id,
        gateway.Name,
        gateway.SupportedMethod.ToString(),
        gateway.IsActive,
        gateway.TimeoutSeconds,
        gateway.TransactionFeePercentage,
        gateway.MinimumAmount,
        gateway.MaximumAmount
    );
}