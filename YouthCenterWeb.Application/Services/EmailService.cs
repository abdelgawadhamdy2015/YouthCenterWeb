using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using YouthCenterWeb.YouthCenterWeb.Application.Interfaces;

namespace YouthCenterWeb.YouthCenterWeb.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly HttpClient _http;
        private readonly ILogger<EmailService> _logger;
        private readonly string _senderEmail;
        private readonly string _senderName;

        public EmailService(
            HttpClient http,
            IConfiguration config,
            ILogger<EmailService> logger)
        {
            _http = http;
            _logger = logger;
            var apiKey = config["Brevo:ApiKey"]
                ?? throw new InvalidOperationException("Brevo:ApiKey is not configured.");

            _logger.LogInformation("Brevo key loaded, length: {Length}, starts: {Prefix}",
                apiKey.Length, apiKey.Length > 10 ? apiKey[..10] : apiKey);
            // var apiKey = config["Brevo:ApiKey"]
            //     ?? throw new InvalidOperationException("Brevo:ApiKey is not configured.");

            _senderEmail = config["Brevo:SenderEmail"] ?? "abdelgawadhamdy93@gmail.com";
            _senderName = config["Brevo:SenderName"] ?? "Youth Center";

            _http.BaseAddress = new Uri("https://api.brevo.com/v3/");
            _http.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            _http.DefaultRequestHeaders.Add("api-key", apiKey);
        }

        public async Task<bool> SendAsync(string email, string subject, string message)
        {
            var payload = new
            {
                sender = new { email = _senderEmail, name = _senderName },
                to = new[] { new { email } },
                subject,
                htmlContent = $"<p>{message}</p>"
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _http.PostAsync("smtp/email", content);

                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning(
                        "Brevo send failed. Status: {Status}, Body: {Body}",
                        response.StatusCode, body);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email via Brevo to {Email}", email);
                return false;
            }
        }
    }
}