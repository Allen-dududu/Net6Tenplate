namespace Net6TemplateWebApi.Infrastructure.Filters
{
    public class JsonErrorResponse
    {
        public string[] Messages { get; set; }
        public string ErrorId { get; set; }

        public object DeveloperMessage { get; set; }
    }

}
