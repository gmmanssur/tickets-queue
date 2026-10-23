using TicketsQueue.Application.UseCases;

namespace TicketsQueue.Application.Commons.Interfaces
{
    public interface ITicketsQueueService
    {
        Task<TicketQueueResult<TicketsQueueResponse>> CreateTicketAsync(TicketsQueueRequest ticket, CancellationToken cancellationToken);
        Task<TicketQueueResult<TicketsQueueResponse>> GetTicketsByIdAsync(Guid id, CancellationToken cancellationToken);
    }
}
