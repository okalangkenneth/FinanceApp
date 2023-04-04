using FinanceApp.Services.IEmailService;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinanceApp.Controllers
{
    public class EmailController : Controller
    {
        private readonly IEmailService _emailService;

        public EmailController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task<IActionResult> SendTestEmail()
        {
            await _emailService.SendEmailAsync("ken@backendinsight.com", "Test email", "This is a test email.");
            return View();
        }
    }
}
