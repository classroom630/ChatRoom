namespace ChatRoom.Models.DTOs
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public int StatusCode { get; set; }
        public List<string> Errors { get; set; } = new List<string>();

        public static ApiResponse<T> SuccessResponse(T data, string? message = null)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data,
                Message = message,
                StatusCode = 200
            };
        }

        public static ApiResponse<T> ErrorResponse(string message, int statusCode = 400, List<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                StatusCode = statusCode,
                Errors = errors ?? new List<string>()
            };
        }
    }

    public class ApiResponse : ApiResponse<object>
    {
        public static ApiResponse SuccessResponse(string? message = null)
        {
            return new ApiResponse
            {
                Success = true,
                Message = message,
                StatusCode = 200
            };
        }

        public static new ApiResponse ErrorResponse(string message, int statusCode = 400, List<string>? errors = null)
        {
            return new ApiResponse
            {
                Success = false,
                Message = message,
                StatusCode = statusCode,
                Errors = errors ?? new List<string>()
            };
        }
    }
}