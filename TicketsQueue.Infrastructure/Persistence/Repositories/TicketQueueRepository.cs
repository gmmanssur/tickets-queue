using TicketsQueue.Application.Commons.Interfaces;
using TicketsQueue.Application.UseCases;
using TicketsQueue.Domain.Entities;
using TicketsQueue.Domain.Enums;
using TicketsQueue.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace TicketsQueue.Infrastructure.Persistence.Repositories
{
    public class TicketQueueRepository(TicketsQueueDbContext TicketsQueueDbContext) : ITicketQueueRepository
    {
        readonly TicketsQueueDbContext _dbContext = TicketsQueueDbContext;

        public async Task<bool> ExistTicketInProcessByUserAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _dbContext.Ticket
                .Where(t => t.UserId == userId && t.Status == TicketStatus.InProgress)
                .AnyAsync(cancellationToken);
        }

        public async Task<Ticket> ExecuteCreateTicketAsync(TicketsQueueRequest ticket, CancellationToken cancellationToken)
        {
            Ticket newTicket = new(ticket.UserId, ticket.Title, ticket.Description, ticket.Priority);

            await _dbContext.Ticket.AddAsync(newTicket, cancellationToken);
            
            await _dbContext.SaveChangesAsync(cancellationToken);

            return newTicket;
        }

        public async Task<Ticket?> ExecuteGetTicketByIdAsync(Guid id, CancellationToken cancellationToken)
            => await _dbContext.Ticket.Where(t => t.UserId == id).FirstOrDefaultAsync(cancellationToken);
    }
}