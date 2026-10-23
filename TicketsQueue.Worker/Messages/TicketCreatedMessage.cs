namespace TicketsQueue.Worker.Messages
{
    public sealed record TicketCreatedMessage(
        Guid TicketId,
        Guid UserId,
        string Title,
        int Priority
    );
}
