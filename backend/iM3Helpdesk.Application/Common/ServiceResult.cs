namespace iM3Helpdesk.Application.Common;

public class ServiceResult
{
    public bool IsSuccess { get; }
    public string? ErrorMessage { get; }
    public int StatusCode { get; }

    protected ServiceResult(bool isSuccess, int statusCode, string? errorMessage)
    {
        IsSuccess = isSuccess;
        StatusCode = statusCode;
        ErrorMessage = errorMessage;
    }

    public static ServiceResult Success(int statusCode = 200) => new(true, statusCode, null);
    public static ServiceResult Failure(string errorMessage, int statusCode = 400) => new(false, statusCode, errorMessage);
    public static ServiceResult NotFound(string message = "Resource not found") => new(false, 404, message);
    public static ServiceResult BadRequest(string message) => new(false, 400, message);
    public static ServiceResult Unauthorized(string message = "Unauthorized") => new(false, 401, message);
}

public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; }

    private ServiceResult(bool isSuccess, int statusCode, string? errorMessage, T? data)
        : base(isSuccess, statusCode, errorMessage)
    {
        Data = data;
    }

    public static ServiceResult<T> Success(T data, int statusCode = 200) => new(true, statusCode, null, data);
    public static new ServiceResult<T> Failure(string errorMessage, int statusCode = 400) => new(false, statusCode, errorMessage, default);
    public static new ServiceResult<T> NotFound(string message = "Resource not found") => new(false, 404, message, default);
    public static new ServiceResult<T> BadRequest(string message) => new(false, 400, message, default);
    public static new ServiceResult<T> Unauthorized(string message = "Unauthorized") => new(false, 401, message, default);
}
