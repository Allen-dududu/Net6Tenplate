using Net6TemplateWebApi.Infrastructure.ActionResults;
using Net6TemplateWebApi.Infrastructure.Exceptions;
using Net6.Template.SDK.Requests;
using Net6.Template.SDK.Responses;

namespace Net6TemplateWebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public ApiResponse<IEnumerable<WeatherForecast>> Get(int id)
        {
            _logger.LogInformation("getForecast");
            var data = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
.ToArray();
            return ApiResponse<IEnumerable<WeatherForecast>>.Success(data);

        }

        [HttpPost(Name = "GetWeatherForecast")]
        public IActionResult Post(PostForecast postForecast)
        {
            _logger.LogInformation("postForecast");
            _logger.LogInformation("endForecast");

            return Ok(new Forecast() { Id = 1, Name = postForecast.Name });
        }
    }
}