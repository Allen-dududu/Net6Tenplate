using Net6TemplateWebApi.Infrastructure.ActionResults;
using Net6.Template.Repertory;

namespace Net6TemplateWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoController : ControllerBase
    {
        private readonly ITodoRepertory _todoRepertory;
        private readonly IOptions<Net6TemplateWebApiSettings> _config;

        public TodoController(ITodoRepertory todoRepertory, IOptions<Net6TemplateWebApiSettings> config)
        {
            _todoRepertory = todoRepertory;
            _config = config;
        }
        [HttpGet]
        public async Task<ApiResponse<dynamic>> getAsync()
        {
            var result = await _todoRepertory.Get();

            return  ApiResponse<dynamic>.Success(new { result, _config.Value.ConnectionString});
        }
    }
}
