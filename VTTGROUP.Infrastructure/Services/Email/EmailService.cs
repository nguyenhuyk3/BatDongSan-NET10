using System.Net.Mail;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using VTTGROUP.Domain.Model.Email;

namespace VTTGROUP.Infrastructure.Services.Email
{
    public interface IEmailService
    {
        Task SendEmailAsync(EmailMessageModal message);
    }
    public class EmailService : IEmailService
    {
        private readonly EmailSettingsModal _settings;

        public EmailService(IOptions<EmailSettingsModal> options)
        {
            _settings = options.Value;
        }

        public async Task SendEmailAsync(EmailMessageModal message)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));

            foreach (var to in message.To)
                email.To.Add(MailboxAddress.Parse(to));

            email.Subject = message.Subject;
            var builder = new BodyBuilder { HtmlBody = message.BodyHtml };

            if (message.Attachments != null)
            {
                foreach (var (bytes, fileName) in message.Attachments)
                {
                    builder.Attachments.Add(fileName, bytes);
                }
            }

            email.Body = builder.ToMessageBody();

            using var smtp = new MailKit.Net.Smtp.SmtpClient();
            await smtp.ConnectAsync(_settings.SmtpServer, _settings.SmtpPort, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_settings.Username, _settings.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}
