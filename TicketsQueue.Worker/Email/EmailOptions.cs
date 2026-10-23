namespace TicketsQueue.Worker.Email
{
    public sealed class EmailOptions
    {
        public const string SectionName = "Email";
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 1025;
        public string FromName { get; set; } = "Tickets Queue";
        public string FromEmail { get; set; } = "tickets@localhost";
        public string ToEmail { get; set; } = "suporte@localhost";
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool UseStartTls { get; set; }
    }
}
