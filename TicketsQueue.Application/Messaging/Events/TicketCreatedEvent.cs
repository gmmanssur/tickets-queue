using TicketsQueue.Domain.Enums;

namespace TicketsQueue.Application.Messaging.Events
{
    public sealed record TicketCreatedEvent(
        Guid TicketId,
        Guid UserId,
        string Title,
        TicketPriority Priority,
        DateTime CreatedAt
    );
}
