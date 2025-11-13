using ProductManagement.Domain.Enum;

namespace ProductManagement.Domain.Entities;

public class PaymentGatewayConfig
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public PaymentMethod SupportedMethod { get; private set; }
    public bool IsActive { get; private set; }
    public string ApiKey { get; private set; }
    public string ApiSecret { get; private set; }
    public string WebhookUrl { get; private set; }
    public int TimeoutSeconds { get; private set; }
    public decimal TransactionFeePercentage { get; private set; }
    public decimal MinimumAmount { get; private set; }
    public decimal MaximumAmount { get; private set; }

    // Constructor for Dapper
    private PaymentGatewayConfig()
    {
        Name = string.Empty;
        ApiKey = string.Empty;
        ApiSecret = string.Empty;
        WebhookUrl = string.Empty;
    }

    public PaymentGatewayConfig(
        string name,
        PaymentMethod supportedMethod,
        string apiKey,
        string apiSecret,
        string webhookUrl)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        SupportedMethod = supportedMethod;
        IsActive = true;
        ApiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        ApiSecret = apiSecret ?? throw new ArgumentNullException(nameof(apiSecret));
        WebhookUrl = webhookUrl ?? throw new ArgumentNullException(nameof(webhookUrl));
        TimeoutSeconds = 30;
        TransactionFeePercentage = 2.5m;
        MinimumAmount = 1.0m;
        MaximumAmount = 100000m;
    }
}