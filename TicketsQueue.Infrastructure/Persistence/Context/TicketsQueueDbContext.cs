using Microsoft.EntityFrameworkCore;
using TicketsQueue.Domain.Entities;

namespace TicketsQueue.Infrastructure.Persistence.Context
{
    public class TicketsQueueDbContext(DbContextOptions<TicketsQueueDbContext> options) : DbContext(options)
    {
        public DbSet<Ticket> Ticket => Set<Ticket>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(TicketsQueueDbContext).Assembly);
        }
    }
}
