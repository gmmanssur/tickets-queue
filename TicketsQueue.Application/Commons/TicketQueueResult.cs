namespace TicketsQueue.Application.Commons
{
    public sealed record class TicketQueueResult<T>
    {
        public bool IsSuccess { get; }
        public T? Data { get; }
        public string? Error { get; }

        private TicketQueueResult(bool isSuccess, T? data, string? error)
        {
            IsSuccess = isSuccess;
            Data = data;
            Error = error;
        }

        public static TicketQueueResult<T> Success(T data)
            => new(true, data, null);

        public static TicketQueueResult<T> Failure(string error)
            => new(false, default, error);
    }
}