namespace GameStore.Dtos;

public class ApiResponse<T>
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }
    public IReadOnlyCollection<string> Errors { get; init; } = [];
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public static ApiResponse<T> Ok(T? data, string message = "Request completed successfully.") =>
        new()
        {
            Success = true,
            Message = message,
            Data = data
        };

    public static ApiResponse<T> Fail(string message, params string[] errors) =>
        new()
        {
            Success = false,
            Message = message,
            Errors = errors.Length == 0 ? [message] : errors
        };
}
