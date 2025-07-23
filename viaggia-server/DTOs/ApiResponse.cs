namespace viaggia_server.DTOs
{
    /// <summary>
    /// Standard API response format.
    /// </summary>
    public class ApiResponse
    {
        public bool Success { get; }
        public string Message { get; }
        public object Data { get; }

        public ApiResponse(bool success, string message, object data = null)
        {
            Success = success;
            Message = message;
            Data = data;
        }
    }
}
