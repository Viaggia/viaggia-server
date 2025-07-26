namespace viaggia_server.DTOs
{
    public class ApiResponse<T>
    {
        public bool Success { get; }
        public string Message { get; }
        public T? Data { get; }
        public object? Errors { get; }

        public ApiResponse(bool success, string message, T? data = default, object? errors = null)
        {
            Success = success;
            Message = message ?? throw new ArgumentNullException(nameof(message));
            Data = data;
            Errors = errors;
        }
    }
}