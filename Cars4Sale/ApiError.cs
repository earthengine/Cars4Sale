using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace Cars4Sale
{
    /// <summary>
    /// A struct for error handling.
    /// 
    /// However this is not the standard .NET exceptions. This is using 
    /// Go style error handling. Since the normal result does not need
    /// to have any data, it will be null.
    ///
    /// Since C# exceptions are unchecked, it is better to show
    /// posible error path in the function signature.
    /// </summary>    
    public struct ApiError
    {
        [JsonIgnore]
        public int StatusCode { get; set; }
        public string Reason { get; set; }

        public static ApiError BadRequest(string reason)
        {
            return new ApiError { StatusCode = StatusCodes.Status400BadRequest, Reason = reason };
        }
        public static ApiError Unauthorized(string reason)
        {
            return new ApiError { StatusCode = StatusCodes.Status401Unauthorized, Reason = reason };
        }
        public static ApiError Forbidden(string reason)
        {
            return new ApiError { StatusCode = StatusCodes.Status403Forbidden, Reason = reason };
        }
        public static ApiError NotFound(string reason)
        {
            return new ApiError { StatusCode = StatusCodes.Status404NotFound, Reason = reason };
        }
        public static ApiError Conflict(string reason)
        {
            return new ApiError { StatusCode = StatusCodes.Status409Conflict, Reason = reason };
        }
        public static ApiError Gone(string reason)
        {
            return new ApiError { StatusCode = StatusCodes.Status410Gone, Reason = reason };
        }
        public static ApiError Internal(string reason)
        {
            return new ApiError { StatusCode = StatusCodes.Status500InternalServerError, Reason = reason };
        }
        
        public ContentResult ToContentResult()
        {
            return new ContentResult { ContentType = "text/plain", Content = Reason, StatusCode = StatusCode };
        }
    }

}
