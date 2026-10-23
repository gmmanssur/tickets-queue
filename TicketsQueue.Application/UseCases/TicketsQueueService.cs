
using TicketsQueue.Application.Commons;
using TicketsQueue.Application.Commons.Interfaces;
using TicketsQueue.Application.Messaging;
using TicketsQueue.Application.Messaging.Events;
using TicketsQueue.Domain.Entities;

namespace TicketsQueue.Application.UseCases;

public sealed class TicketsQueueService(ITicketQueueRepository ticketQueueRepository, IMessagePublisher messagePublisher) : ITicketsQueueService
{
    private readonly ITicketQueueRepository _ticketQueueRepository = ticketQueueRepository;
    private readonly IMessagePublisher _messagePublisher = messagePublisher;

    public async Task<TicketQueueResult<TicketsQueueResponse>> CreateTicketAsync(
        TicketsQueueRequest ticket,
        CancellationToken cancellationToken)
    {
        bool ticketInProcess = await _ticketQueueRepository.ExistTicketInProcessByUserAsync(
            ticket.UserId,
            cancellationToken
        );

        if (ticketInProcess)
            return TicketQueueResult<TicketsQueueResponse>.Failure("There is already a request in process for this user.");

        Ticket createdTicket = await _ticketQueueRepository.ExecuteCreateTicketAsync(ticket, cancellationToken);

        var ticketCreatedEvent = new TicketCreatedEvent(
            createdTicket.Id,
            createdTicket.UserId,
            createdTicket.Title,
            createdTicket.Priority,
            createdTicket.CreatedAt);

        await _messagePublisher.PublishAsync(
            ticketCreatedEvent,
            cancellationToken);

        var response = new TicketsQueueResponse(
            createdTicket.Id,
            createdTicket.UserId,
            createdTicket.Title,
            createdTicket.Description,
            createdTicket.Status,
            createdTicket.Priority,
            createdTicket.CreatedAt,
            createdTicket.UpdatedAt);

        return TicketQueueResult<TicketsQueueResponse>.Success(response);
    }

    public async Task<TicketQueueResult<TicketsQueueResponse>> GetTicketsByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        Ticket? ticket =await _ticketQueueRepository.ExecuteGetTicketByIdAsync(id, cancellationToken);

        if (ticket is null)
        {
            return TicketQueueResult<TicketsQueueResponse>.Failure("Ticket not found.");
        }

        TicketsQueueResponse response = new(
            ticket.Id,
            ticket.UserId,
            ticket.Title,
            ticket.Description,
            ticket.Status,
            ticket.Priority,
            ticket.CreatedAt,
            ticket.UpdatedAt);

        return TicketQueueResult<TicketsQueueResponse>.Success(response);
    }
}
