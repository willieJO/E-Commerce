using e_commerce.Infra.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace e_commerce.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly DatabaseService _DatabaseService;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, DatabaseService databaseService)
        {
            _DatabaseService = databaseService;
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IActionResult> Get()
        {

            try
            {
                var select = $@"SELECT * FROM teste";
                var q = $@"insert into teste
                            values
                            (20,'t2')";
                var q2 = $@"insert into teste
                            values
                            (1,'tdqwdqwdqwdqwdqwdqwdqwdqwdq')";
                var result = await _DatabaseService.ExecuteNonQueryAsync(q, true);

                // Comita a transação após o sucesso
                var retorno = await _DatabaseService.ExecuteQueryAsync<Teste>(select);

                return Ok(retorno);
            }
            catch (Exception e)
            {
                // Em caso de erro, desfaz a transação
                await _DatabaseService.RollbackTransactionAsync();
                return BadRequest(e.StackTrace);
            }
        }
        public class Teste
        {
            public int Id { get; set; }
            public string Nome { get; set; }
        }
    }
}
