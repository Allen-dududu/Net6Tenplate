using Net6TemplateWebApi.Infrastructure.ActionResults;
using Net6TemplateWebApi.Repertory;

namespace Net6TemplateWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TodoController : ControllerBase
    {
        private readonly ITodoRepertory _todoRepertory;


        public TodoController(ITodoRepertory todoRepertory)
        {
            _todoRepertory = todoRepertory;
        }
        [HttpGet]
        public async Task<ApiResponse<Todo>> getAsync()
        {
            var result = await _todoRepertory.Get();

            return  ApiResponse<Todo>.Success(result);
        }
    }
}
