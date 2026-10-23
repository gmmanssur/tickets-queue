using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using TicketsQueue.Application.Messaging;

namespace TicketsQueue.Infrastructure.Messaging;

public sealed class RabbitMqPublisher(IOptions<RabbitMqOptions> options) : IMessagePublisher
{
    readonly RabbitMqOptions _options = options.Value;

    public async Task PublishAsync<T>(
        T message,
        CancellationToken cancellationToken)
    {
        ConnectionFactory factory = new()
        {
            HostName = _options.Host,
            Port = _options.Port,
            UserName = _options.Username,
            Password = _options.Password
        };

        await using var connection = await factory.CreateConnectionAsync(cancellationToken);

        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            exchange: _options.Exchange,
            type: ExchangeType.Fanout,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken
        );

        byte[]? body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        await channel.BasicPublishAsync(
            exchange: _options.Exchange,
            routingKey: string.Empty,
            body: body,
            cancellationToken: cancellationToken
        );
    }
}