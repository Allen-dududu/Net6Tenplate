namespace Net6TemplateWebApi.Infrastructure.ActionResults
{
    public class ApiResponse<T>
    {
        public T Data { get; set; }
        public bool Succeeded { get; set; }
        public string Message { get; set; }

        public string ErrorId { get; set; }

        public static ApiResponse<T> Fail(string errorMessage)
        {
            return new ApiResponse<T> { Succeeded = false, Message = errorMessage };
        }

        public static ApiResponse<T> Fail(string errorMessage, string ErrorId)
        {
            return new ApiResponse<T> { Succeeded = false, Message = errorMessage,ErrorId = ErrorId };
        }

        public static ApiResponse<T> Success(T data)
        {
            return new ApiResponse<T> { Succeeded = true, Data = data};
        }
    }
}
