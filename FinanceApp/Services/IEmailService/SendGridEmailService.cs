using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;
using FinanceApp.Services.IEmailService;

public class SendGridEmailService : IEmailService
{
    private readonly string _apiKey;
    private readonly ILogger<SendGridEmailService> _logger;

    public SendGridEmailService(string apiKey, ILogger<SendGridEmailService> logger)
    {
        _apiKey = apiKey;
        _logger = logger;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        // SendGridClient throws ArgumentNullException on a missing key, which
        // turned registration into a 500 after the user row was already
        // created. Without a key, skip the send so the flow completes; the
        // user just receives no confirmation email. Send-result handling and
        // client reuse are Phase 3 (audit item 43).
        if (string.IsNullOrEmpty(_apiKey))
        {
            _logger.LogWarning("[SendGridEmailService] SendGrid:ApiKey not configured — skipping email '{Subject}' to {Email}", subject, email);
            return;
        }

        var client = new SendGridClient(_apiKey);
        var msg = new SendGridMessage()
        {
            From = new EmailAddress("ken@backendinsight.com", "FinTrak"),
            Subject = subject,
            PlainTextContent = htmlMessage,
            HtmlContent = htmlMessage
        };
        msg.AddTo(new EmailAddress(email));

        await client.SendEmailAsync(msg);
    }
}
