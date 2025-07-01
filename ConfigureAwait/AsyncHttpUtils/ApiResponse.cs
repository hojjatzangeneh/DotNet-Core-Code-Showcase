namespace AsyncHttpUtils;

public class ApiResponse <T>
{
    public static ApiResponse<T> Failure(string error) { return new() { IsSuccess = false, ErrorMessage = error }; }

    public static ApiResponse<T> Success(T data) { return new() { IsSuccess = true, Data = data }; }

    public T? Data { get; init; }

    public string? ErrorMessage { get; init; }

    public bool IsSuccess { get; init; }
}
