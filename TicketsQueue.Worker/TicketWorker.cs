using RabbitMQ.Client;
using TicketsQueue.Worker.Consumers;
using TicketsQueue.Worker.Email;
using TicketsQueue.Worker.RabbitMq;

namespace TicketsQueue.Worker
{
    public sealed class TicketWorker(
        RabbitMqConnection rabbitMqConnection,
        IEmailSender emailSender,
        Microsoft.Extensions.Options.IOptions<EmailOptions> emailOptions,
        IConfiguration configuration) : BackgroundService
    {
        private readonly RabbitMqConnection _rabbitMqConnection = rabbitMqConnection;
        private readonly IEmailSender _emailSender = emailSender;
        private readonly Microsoft.Extensions.Options.IOptions<EmailOptions> _emailOptions = emailOptions;
        private readonly IConfiguration _configuration = configuration;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            IConnection connection = await _rabbitMqConnection.CreateAsync(stoppingToken);

            TicketCreatedConsumer consumer = new(
                connection,
                _emailSender,
                _emailOptions,
                _configuration
            );

            await consumer.StartAsync(stoppingToken);
        }
    }
}