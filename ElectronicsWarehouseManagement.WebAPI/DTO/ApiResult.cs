using System.Text.Json.Serialization;

public class ApiResult<T>
{
    public ApiResult(T? data = default)
    {
        Success = true;
        Data = data;
    }

    public ApiResult(ApiResultCode errorCode, string errorMessage, T? data = default)
    {
        Success = false;
        ResultCode = errorCode;
        Message = errorMessage;
        Data = data;
    }


    [JsonPropertyName("code")]
    public ApiResultCode ResultCode { get; }
    
    [JsonPropertyName("msg")]
    public string Message { get; } = "Success";
    
    [JsonPropertyName("data")]
    public T? Data { get; }

    [JsonPropertyName("success")]
    public bool Success { get; }
}

public class ApiResult
{
    public ApiResult()
    {
        Success = true;
    }

    public ApiResult(ApiResultCode errorCode, string errorMessage)
    {
        Success = false;
        ResultCode = errorCode;
        Message = errorMessage;
    }


    [JsonPropertyName("success")]
    public bool Success { get; }
    
    [JsonPropertyName("msg")]
    public string Message { get; } = "Success";

    [JsonPropertyName("code")]
    public ApiResultCode ResultCode { get; }
}

public enum ApiResultCode
{
    Success,
    NotFound,
    AlreadyLoggedIn,
    InvalidRequest,
    IncorrectCred,
    UnknownError = -1,
}