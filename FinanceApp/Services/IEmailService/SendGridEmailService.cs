using FinanceApp.Configuration;
using FinanceApp.Services.IEmailService;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

public class SendGridEmailService : IEmailService
{
    private readonly SendGridOptions _options;
    private readonly ILogger<SendGridEmailService> _logger;

    // One client for the service lifetime (the old code built one per send);
    // null when no API key is configured — sends are skipped with a warning
    // so flows like registration still complete (Phase 2 decision).
    private readonly ISendGridClient _client;

    public SendGridEmailService(IOptions<SendGridOptions> options, ILogger<SendGridEmailService> logger)
    {
        _options = options.Value;
        _logger = logger;
        _client = string.IsNullOrEmpty(_options.ApiKey) ? null : new SendGridClient(_options.ApiKey);
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        if (_client == null)
        {
            _logger.LogWarning("[SendGridEmailService] SendGrid:ApiKey not configured — skipping email '{Subject}' to {Email}", subject, email);
            return;
        }

        var msg = new SendGridMessage()
        {
            From = new EmailAddress(_options.SenderEmail, _options.SenderName),
            Subject = subject,
            PlainTextContent = htmlMessage,
            HtmlContent = htmlMessage
        };
        msg.AddTo(new EmailAddress(email));

        var response = await _client.SendEmailAsync(msg);
        if (!response.IsSuccessStatusCode)
        {
            string body = await response.Body.ReadAsStringAsync();
            _logger.LogError(
                "[SendGridEmailService] Send '{Subject}' to {Email} failed with {StatusCode}. Body: {Body}",
                subject, email, (int)response.StatusCode, body);
        }
        else
        {
            _logger.LogInformation("[SendGridEmailService] Sent '{Subject}' to {Email}", subject, email);
        }
    }
}
