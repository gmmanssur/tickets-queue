using RabbitMQ.Client;

namespace TicketsQueue.Worker.RabbitMq
{
    public sealed class RabbitMqConnection
    {
        private readonly ConnectionFactory _factory;

        public RabbitMqConnection(IConfiguration configuration)
        {
            _factory = new ConnectionFactory
            {
                HostName = configuration["RabbitMq:HostName"] ?? "localhost",
                Port = int.TryParse(configuration["RabbitMq:Port"], out var port)
                    ? port
                    : 5672,
                UserName = configuration["RabbitMq:UserName"] ?? "guest",
                Password = configuration["RabbitMq:Password"] ?? "guest"
            };
        }

        public Task<IConnection> CreateAsync(CancellationToken cancellationToken = default)
            => _factory.CreateConnectionAsync(cancellationToken);
    }
}