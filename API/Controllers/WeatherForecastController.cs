using Assinador;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
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
        public string Get()
        {
            //var response = AssinarPDF.Sign("", "", "C:\\Users\\0308\\Desktop\\Calendario-Matricula.pdf");

            //var response = AssinarPDF.Sign("", "", "D:\\Desktop\\Outros\\Controle_Interno.pdf");

            string tempFileName = System.IO.Path.Combine("/tmp", System.IO.Path.GetRandomFileName());
            string filename = System.IO.Path.GetTempFileName();

            return $"1: {tempFileName} 2: {filename}";

            //return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            //{
            //    Date = DateTime.Now.AddDays(index),
            //    TemperatureC = Random.Shared.Next(-20, 55),
            //    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            //})
            //.ToArray();
        }
    }
}