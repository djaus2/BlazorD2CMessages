using BlazorApp1.Data;
using BlazorD2CMessages.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorD2CMessages.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SensorController : ControllerBase
    {


        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<SensorController> logger;

        public SensorController(ILogger<SensorController> logger)
        {
            this.logger = logger;
        }

        //[HttpGet]
        //public  IEnumerable<Sensor> Get()
        //{

        //    var xx =  ReadDeviceToCloudMessages.Sensors;
        //    return xx;
      
        //    //var rng = new Random();
        //    //return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        //    //{
        //    //    Date = DateTime.Now.AddDays(index),
        //    //    TemperatureC = rng.Next(-20, 55),
        //    //    Summary = Summaries[rng.Next(Summaries.Length)]
        //    //})
        //    //.ToArray();
        //}

        [HttpGet]
        public  async Task<IActionResult> Get()
        {
            List<Sensor> Sensors = new List<Sensor>();
            while(ReadDeviceToCloudMessages.Sensors.Count!=0)
            {
                Sensor sensor = ReadDeviceToCloudMessages.Sensors.Dequeue();
                Sensors.Add(sensor);
            }
            //var sensors = ReadDeviceToCloudMessages.Sensors;
            await Task.Delay(1000);
            return Ok(Sensors);
        }
    }
}
