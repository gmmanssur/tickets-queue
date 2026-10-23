
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TicketsQueue.Worker.Email;
using TicketsQueue.Worker.Messages;

namespace TicketsQueue.Worker.Consumers;

public sealed class TicketCreatedConsumer(
    IConnection connection,
    IEmailSender emailSender,
    IOptions<EmailOptions> emailOptions,
    IConfiguration configuration)
{
    private const string QueueName = "tickets.created";

    private readonly IConnection _connection = connection;
    private readonly IEmailSender _emailSender = emailSender;
    private readonly EmailOptions _emailOptions = emailOptions.Value;

    private readonly string _exchangeName = "tickets" ?? throw new InvalidOperationException("RabbitMq config: Exchange not defined.");

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            exchange: _exchangeName,
            type: ExchangeType.Fanout,
            durable: true,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken
        );

        await channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken
        );

        await channel.QueueBindAsync(
            queue: QueueName,
            exchange: _exchangeName,
            routingKey: string.Empty,
            cancellationToken: cancellationToken
        );

        Console.WriteLine($"Queue '{QueueName}' connected to exchange '{_exchangeName}'.");

        AsyncEventingBasicConsumer consumer = new(channel);

        consumer.ReceivedAsync += async (_, eventArgs) =>
        {
            try
            {
                byte[] body = eventArgs.Body.ToArray();
                string json = Encoding.UTF8.GetString(body);

                TicketCreatedMessage? message = JsonSerializer.Deserialize<TicketCreatedMessage>(json);

                if (message is null)
                {
                    Console.WriteLine("Invalid message: not was possible desserialize this ticket.");

                    await channel.BasicNackAsync(
                        deliveryTag: eventArgs.DeliveryTag,
                        multiple: false,
                        requeue: false,
                        cancellationToken: cancellationToken);

                    return;
                }

                Console.WriteLine("Ticket recebido:");
                Console.WriteLine($"Id: {message.TicketId}");
                Console.WriteLine($"UserId: {message.UserId}");
                Console.WriteLine($"Title: {message.Title}");
                Console.WriteLine($"Priority: {message.Priority}");

                string subject = $"New ticket created - {message.TicketId}";

                string bodyText = $"""
                    A new ticket has been created.

                    ID: {message.TicketId}
                    User: {message.UserId}
                    Title: {message.Title}
                    Priority: {message.Priority}

                    Please verify the ticket.
                    """;

                await _emailSender.SendAsync(
                    to: _emailOptions.ToEmail,
                    subject: subject,
                    body: bodyText,
                    cancellationToken: cancellationToken
                );

                Console.WriteLine($"Email accepted by SMTP for {_emailOptions.ToEmail}.");

                await channel.BasicAckAsync(
                    deliveryTag: eventArgs.DeliveryTag,
                    multiple: false,
                    cancellationToken: cancellationToken
                );
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing ticket or sending email: {ex}");

                await channel.BasicNackAsync(
                    deliveryTag: eventArgs.DeliveryTag,
                    multiple: false,
                    requeue: true,
                    cancellationToken: cancellationToken
                );
            }
        };

        await channel.BasicConsumeAsync(
            queue: QueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: cancellationToken
        );

        Console.WriteLine($"Worker waiting for messages in queue '{QueueName}'...");

        await Task.Delay(Timeout.Infinite, cancellationToken);
    }
}
