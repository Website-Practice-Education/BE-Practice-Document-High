using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Website_Documents.Service.Interfaces;

public interface IEmailService
{
    Task<bool> SendEmailAsync(string to, string subject, string htmlBody);
    Task<bool> SendPasswordResetEmailAsync(string to, string resetLink);
}
