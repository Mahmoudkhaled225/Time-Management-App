using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace src.Services;

public class EmailService : IEmailService
{
    private readonly Email _options;

    public EmailService(IOptions<Email> options) =>
        _options = options.Value;
    
    
    public async Task<bool> SendEmail(string to, string subject, string body)
    {
        try
        {
            var mail = new MimeMessage
            {
                Sender = MailboxAddress.Parse(_options.From),
                Subject = subject
            };

            mail.To.Add(MailboxAddress.Parse(to));
            mail.From.Add(new MailboxAddress(_options.DisplayName, _options.From));

            var builder = new BodyBuilder { TextBody = body };
            mail.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_options.Host, _options.Port, MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_options.From, _options.Password);

            await smtp.SendAsync(mail);

            await smtp.DisconnectAsync(true);

            return true; // Return true if the email was sent successfully
        }
        catch (Exception)
        {
            return false; // Return false if the email sending operation failed
        }
    }

}