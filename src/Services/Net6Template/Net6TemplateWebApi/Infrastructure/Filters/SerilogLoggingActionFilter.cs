namespace Net6TemplateWebApi.Infrastructure.Filters;
public class SerilogLoggingActionFilter : IActionFilter
{
    private readonly IDiagnosticContext _diagnosticContext;
    public SerilogLoggingActionFilter(IDiagnosticContext diagnosticContext)
    {
        _diagnosticContext = diagnosticContext;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        _diagnosticContext.Set("ValidationState", context.ModelState.IsValid);
        _diagnosticContext.Set("ActionId", context.ActionDescriptor.Id);

    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}

