using TicketsQueue.Application.UseCases;
using TicketsQueue.Domain.Entities;

namespace TicketsQueue.Application.Commons.Interfaces
{
    public interface ITicketQueueRepository
    {
        Task<bool> ExistTicketInProcessByUserAsync(Guid userId, CancellationToken cancellationToken);

        Task<Ticket> ExecuteCreateTicketAsync(TicketsQueueRequest ticket, CancellationToken cancellationToken);

        Task<Ticket?> ExecuteGetTicketByIdAsync(Guid id, CancellationToken cancellationToken);
    }
}
