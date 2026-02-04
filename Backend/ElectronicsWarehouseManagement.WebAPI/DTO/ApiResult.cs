public class ApiResult<T>
{
    public ApiResult(T? data = default)
    {
        Success = true;
        Data = data;
    }

    public ApiResult(int errorCode, string errorMessage, T? data = default)
    {
        Success = false;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
        Data = data;
    }

    //TODO: map error codes to enum
    public int ErrorCode { get; }
    public string ErrorMessage { get; } = "Success";
    public T? Data { get; }
    public bool Success { get; }
}

public class ApiResult
{
    public ApiResult()
    {
        Success = true;
    }

    public ApiResult(int errorCode, string errorMessage)
    {
        Success = false;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public bool Success { get; }
    public string ErrorMessage { get; } = "Success";
    //TODO: map error codes to enum
    public int ErrorCode { get; }
}