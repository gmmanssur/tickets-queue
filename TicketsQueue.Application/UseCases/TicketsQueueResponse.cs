using TicketsQueue.Domain.Enums;

namespace TicketsQueue.Application.UseCases
{
    public sealed record TicketsQueueResponse(
        Guid Id,
        Guid UserId,
        string Title,
        string Description,
        TicketStatus Status,
        TicketPriority Priority,
        DateTime CreatedAt,
        DateTime? UpdatedAt
    );
}