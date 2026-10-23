using TicketsQueue.Domain.Enums;

namespace TicketsQueue.Domain.Entities
{
    public class Ticket
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public string Title { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public TicketStatus Status { get; private set; }
        public TicketPriority Priority { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; }

        private Ticket() { }

        public Ticket(
            Guid userId,
            string title,
            string description,
            TicketPriority priority)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            Title = title;
            Description = description;
            Priority = priority;
            Status = TicketStatus.Open;
            CreatedAt = DateTime.UtcNow;
        }
    }
}
