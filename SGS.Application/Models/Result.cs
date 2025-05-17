namespace SGS.Application.Models;

public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Data { get; set; }
    public string? Error { get; }

    protected Result(bool isSuccess, T? data, string? error)
    {
        IsSuccess = isSuccess;
        Data = data;
        Error = error;
    }

    public static Result<T> Success(T data) => new(true, data, null);
    public static Result<T> Failure(string error) => new(false, default, error);
}