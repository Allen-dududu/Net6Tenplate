using Net6TemplateWebApi.Infrastructure.Exceptions;

using Net6TemplateWebApi.Infrastructure.ActionResults;

namespace Net6TemplateWebApi.Infrastructure.Middlewares
{
    public class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlerMiddleware> _logger;
        readonly IDiagnosticContext _diagnosticContext;
        public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger, IDiagnosticContext diagnosticContext)
        {
            _next = next;
            _logger = logger;
            _diagnosticContext = diagnosticContext;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception error)
            {
                var errorId= Guid.NewGuid().ToString();
                _diagnosticContext.Set("ErrorId", errorId);
                _diagnosticContext.Set("ErrorMessage", error);

                var response = context.Response;
                response.ContentType = "application/json";
                var responseModel = ApiResponse<string>.Fail(error.Message, errorId);

                switch (error)
                {
                    case Net6TemplateWebApiDomainException e:
                        // custom application error
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        break;

                    case KeyNotFoundException e:
                        // not found error
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                        break;

                    default:
                        // unhandled error
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        break;
                }
                var result = JsonSerializer.Serialize(responseModel);
                await response.WriteAsync(result);
            }
        }
    }
}
