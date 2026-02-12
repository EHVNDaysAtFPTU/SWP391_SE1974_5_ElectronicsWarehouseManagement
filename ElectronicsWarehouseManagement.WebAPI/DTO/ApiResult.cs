using System.ComponentModel;
using System.Text.Json.Serialization;

public class ApiResult<T>
{
    public ApiResult(T? data = default)
    {
        Success = true;
        Data = data;
    }

    public ApiResult(ApiResultCode errorCode, string errorMessage = null, T? data = default)
    {
        Success = false;
        ResultCode = errorCode;
        if (!string.IsNullOrEmpty(errorMessage))
            Message = errorMessage;
        else
        {
            string name = errorCode.ToString();
            DescriptionAttribute? descriptionAttribute = typeof(ApiResultCode).GetMember(name)[0]
                .GetCustomAttributes(typeof(DescriptionAttribute), false)
                .FirstOrDefault() as DescriptionAttribute;
            if (descriptionAttribute is not null)
                Message = descriptionAttribute.Description;
            else
                Message = name;
        }
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

    public ApiResult(ApiResultCode errorCode, string errorMessage = "")
    {
        Success = false;
        ResultCode = errorCode;
        if (!string.IsNullOrEmpty(errorMessage))
            Message = errorMessage;
        else
        {
            string name = errorCode.ToString();
            DescriptionAttribute? descriptionAttribute = typeof(ApiResultCode).GetMember(name)[0]
                .GetCustomAttributes(typeof(DescriptionAttribute), false)
                .FirstOrDefault() as DescriptionAttribute;
            if (descriptionAttribute is not null)
                Message = descriptionAttribute.Description;
            else
                Message = name;
        }
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
    [Description("Not found")]
    NotFound,
    [Description("Already logged in")]
    AlreadyLoggedIn,
    [Description("Invalid request")]
    InvalidRequest,
    [Description("Incorrect credentials")]
    IncorrectCred,
    [Description("Unknown error")]
    UnknownError = -1,
}