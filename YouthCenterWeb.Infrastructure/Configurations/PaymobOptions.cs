namespace YouthCenterWeb.YouthCenterWeb.Infrastructure.Configurations
{
    public class PaymobOptions
    {
        public const string SectionName = "Paymob";

        public string ApiKey { get; init; } = string.Empty;

        public string IntegrationId { get; init; } = string.Empty;

        public string IframeId { get; init; } = string.Empty;

        public string HmacSecret { get; init; } = string.Empty;
    }
}