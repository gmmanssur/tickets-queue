using TicketsQueue.Domain.Enums;

namespace TicketsQueue.Application.UseCases
{
    public sealed record TicketsQueueRequest(string Title, string Description, Guid UserId, TicketPriority Priority);
}