﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinanceApp.Services.IEmailService
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string message);

    }
}
