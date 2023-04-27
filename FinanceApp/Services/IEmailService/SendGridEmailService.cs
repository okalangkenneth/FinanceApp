using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;
using FinanceApp.Services.IEmailService;

public class SendGridEmailService : IEmailService
{
    private readonly string _apiKey;

    public SendGridEmailService(string apiKey)
    {
        _apiKey = apiKey;
    }

    public async Task<SendGrid.Response> SendEmailAsync(string email, string subject, string htmlMessage)
{
    var client = new SendGridClient(_apiKey);
    var msg = new SendGridMessage()
    {
        From = new EmailAddress("ken@backendinsight.com", "FinTrak"),
        Subject = subject,
        PlainTextContent = htmlMessage,
        HtmlContent = htmlMessage
    };
    msg.AddTo(new EmailAddress(email));

    var response = await client.SendEmailAsync(msg);
    return response;
}

}
