using Serilog.Events;

namespace Net6TemplateWebApi.Infrastructure.Serilog
{
    public static class LogHelper
    {
        public static async void EnrichFromRequest(
            IDiagnosticContext diagnosticContext, HttpContext httpContext)
        {
            var request = httpContext.Request;
            diagnosticContext.Set("Host", request.Host);
            diagnosticContext.Set("Protocol", request.Protocol);
            diagnosticContext.Set("Scheme", request.Scheme);
            if (request.QueryString.HasValue)
            {
                diagnosticContext.Set("QueryString", request.QueryString.Value);
            }

            diagnosticContext.Set("ContentType", httpContext.Response.ContentType);

            var endpoint = httpContext.GetEndpoint();
            if (endpoint is object)
            {
                diagnosticContext.Set("EndpointName", endpoint.DisplayName);
            }
        }

        public static LogEventLevel ExcludeHealthChecks(HttpContext ctx, double _, Exception ex) =>
        ex != null
            ? LogEventLevel.Error
            : ctx.Response.StatusCode > 499
                ? LogEventLevel.Error
                : IsHealthCheckEndpoint(ctx) // Not an error, check if it was a health check
                ? LogEventLevel.Verbose // Was a health check, use Verbose
                : LogEventLevel.Information;


        /// <summary>
        /// Create a <see cref="Serilog.AspNetCore.RequestLoggingOptions.GetLevel"> method that
        /// uses the default logging level, except for the specified endpoint names, which are
        /// logged using the provided <paramref name="traceLevel" />.
        /// </summary>
        /// <param name="traceLevel">The level to use for logging "trace" endpoints</param>
        /// <param name="traceEndpointNames">The display name of endpoints to be considered "trace" endpoints</param>
        /// <returns></returns>
        public static Func<HttpContext, double, Exception, LogEventLevel> GetLevel(LogEventLevel traceLevel, params string[] traceEndpointNames)
        {
            if (traceEndpointNames is null || traceEndpointNames.Length == 0)
            {
                throw new ArgumentNullException(nameof(traceEndpointNames));
            }

            return (ctx, _, ex) =>
                IsError(ctx, ex)
                ? LogEventLevel.Error
                : IsTraceEndpoint(ctx, traceEndpointNames)
                    ? traceLevel
                    : LogEventLevel.Debug;
        }

        static bool IsError(HttpContext ctx, Exception ex)
            => ex != null || ctx.Response.StatusCode > 499;

        static bool IsTraceEndpoint(HttpContext ctx, string[] traceEndpoints)
        {
            var endpoint = ctx.GetEndpoint();
            if (endpoint is object)
            {
                for (var i = 0; i < traceEndpoints.Length; i++)
                {
                    if (string.Equals(traceEndpoints[i], endpoint.DisplayName, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        private static bool IsHealthCheckEndpoint(HttpContext ctx)
        {
            var endpoint = ctx.GetEndpoint();
            if (endpoint is object) // same as !(endpoint is null)
            {
                return string.Equals(
                    endpoint.DisplayName,
                    "Health checks",
                    StringComparison.Ordinal);
            }
            // No endpoint, so not a health check endpoint
            return false;
        }

        private static async Task<string> ReadBodyFromRequest(HttpRequest request)
        {
            // Ensure the request's body can be read multiple times (for the next middlewares in the pipeline).
            request.EnableBuffering();

            using var streamReader = new StreamReader(request.Body, leaveOpen: true);
            var requestBody = await streamReader.ReadToEndAsync();

            // Reset the request's body stream position for next middleware in the pipeline.
            request.Body.Position = 0;
            return requestBody;
        }
    }
}
