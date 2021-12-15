namespace Net6TemplateWebApi.Infrastructure.Exceptions
{
    public class Net6TemplateWebApiDomainException : Exception
    {
        public Net6TemplateWebApiDomainException()
        { }

        public Net6TemplateWebApiDomainException(string message)
            : base(message)
        { }

        public Net6TemplateWebApiDomainException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
