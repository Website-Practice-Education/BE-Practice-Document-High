using System;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Website_Documents.Service.Interfaces;

namespace Website_Documents.Service;

public class EmailService : IEmailService
{
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly string _fromEmail;
    private readonly bool _enableEmail;

    public EmailService(IConfiguration configuration)
    {
        _smtpHost = configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
        _smtpPort = int.Parse(configuration["Email:SmtpPort"] ?? "587");
        _smtpUsername = configuration["Email:Username"] ?? "";
        _smtpPassword = configuration["Email:Password"] ?? "";
        _fromEmail = configuration["Email:FromEmail"] ?? _smtpUsername;
        _enableEmail = bool.Parse(configuration["Email:Enabled"] ?? "false");
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string htmlBody)
    {
        if (!_enableEmail)
        {
            Console.WriteLine($"[EMAIL DISABLED] To: {to}, Subject: {subject}");
            return true;
        }

        try
        {
            using var client = new SmtpClient(_smtpHost, _smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(_smtpUsername, _smtpPassword)
            };

            var mail = new MailMessage
            {
                From = new MailAddress(_fromEmail, "Practice High Edu Doc"),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            mail.To.Add(to);

            await client.SendMailAsync(mail);
            Console.WriteLine($"[EMAIL SENT] To: {to}, Subject: {subject}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EMAIL ERROR] {ex.Message}");
            return false;
        }
    }

    public async Task<bool> SendPasswordResetEmailAsync(string to, string resetLink)
    {
        var subject = "Khôi phục mật khẩu - Practice High Edu Doc";
        var htmlBody = $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 20px; }}
                    .container {{ max-width: 600px; margin: 0 auto; background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
                    .header {{ text-align: center; margin-bottom: 30px; }}
                    .logo {{ font-size: 24px; font-weight: bold; color: #4a90d9; }}
                    .content {{ font-size: 16px; color: #333; line-height: 1.6; }}
                    .button {{ display: inline-block; padding: 15px 30px; background-color: #4a90d9; color: white !important; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
                    .button:hover {{ background-color: #357abd; }}
                    .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #eee; font-size: 12px; color: #666; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <div class='logo'>📚 Practice High Edu Doc</div>
                    </div>
                    <div class='content'>
                        <h2>Yêu cầu khôi phục mật khẩu</h2>
                        <p>Xin chào,</p>
                        <p>Chúng tôi đã nhận được yêu cầu khôi phục mật khẩu cho tài khoản của bạn.</p>
                        <p>Click vào nút bên dưới để đặt lại mật khẩu:</p>
                        <p style='text-align: center;'>
                            <a href='{resetLink}' class='button'>Khôi phục mật khẩu</a>
                        </p>
                        <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
                        <p>Link sẽ hết hạn sau 24 giờ.</p>
                    </div>
                    <div class='footer'>
                        <p>Email này được gửi tự động từ Practice High Edu Doc.</p>
                        <p>© 2026 Practice High Edu Doc. All rights reserved.</p>
                    </div>
                </div>
            </body>
            </html>";

        return await SendEmailAsync(to, subject, htmlBody);
    }
}
