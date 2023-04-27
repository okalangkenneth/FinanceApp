using SendGrid;
using System.Threading.Tasks;

namespace FinanceApp.Services.IEmailService
{
    public interface IEmailService
    {
        Task<Response> SendEmailAsync(string email, string subject, string htmlMessage);
    }
}

