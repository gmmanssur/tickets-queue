using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace TicketsQueue.Worker.Email
{
    public sealed class SmtpEmailSender(IOptions<EmailOptions> options) : IEmailSender
    {
        private readonly EmailOptions _options = options.Value;

        public async Task SendAsync(
            string to,
            string subject,
            string body,
            CancellationToken cancellationToken = default)
        {
            MimeMessage message = new();

            message.From.Add(
                new MailboxAddress(
                    _options.FromName,
                    _options.FromEmail
                )
            );

            message.To.Add(MailboxAddress.Parse(to));

            message.Subject = subject;

            message.Body = new TextPart("plain")
            {
                Text = body
            };

            using SmtpClient client = new();

            SecureSocketOptions socketOptions = _options.UseStartTls
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.None;

            await client.ConnectAsync(
                _options.Host,
                _options.Port,
                socketOptions,
                cancellationToken
            );

            if (!string.IsNullOrWhiteSpace(_options.Username))
            {
                await client.AuthenticateAsync(
                    _options.Username,
                    _options.Password,
                    cancellationToken
                );
            }

            await client.SendAsync(message, cancellationToken);

            await client.DisconnectAsync(quit: true, cancellationToken);
        }
    }
}
