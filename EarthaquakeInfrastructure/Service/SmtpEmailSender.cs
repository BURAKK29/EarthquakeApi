using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace EarthaquakeInfrastructure.Service
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        public SmtpEmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var smtp = _configuration.GetSection("EmailSettings");

            Console.WriteLine($"[EMAIL] Gönderiliyor: {email} | Konu: {subject}");

            using var smtpClient = new SmtpClient(smtp["SmtpServer"])
            {
                Port = int.Parse(smtp["Port"]!),
                Credentials = new NetworkCredential(smtp["Username"], smtp["Password"]),
                EnableSsl = true,
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(smtp["SenderEmail"]!, smtp["SenderName"]),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };

            mailMessage.To.Add(email);
            await smtpClient.SendMailAsync(mailMessage);

            Console.WriteLine($"[EMAIL] Başarıyla gönderildi: {email}");
        }
    }
}
