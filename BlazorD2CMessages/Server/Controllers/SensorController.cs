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

        //private static int delay = 1;
        //private static int delayMax = 16;

        [HttpGet]
        public  async Task<IActionResult> Get()
        {

            List<Sensor> Sensors = new List<Sensor>();
            while(ReadDeviceToCloudMessages.Sensors.Count!=0)
            {
                Sensor sensor = ReadDeviceToCloudMessages.Sensors.Dequeue();
                Sensors.Add(sensor);
            }
            //if (Sensors.Count() == 0)
            //{
            //    if (delay < delayMax)
            //        delay *= 2;
            //}
            //else
            //{
            //    if (delay > 1)
            //        delay /= 2;
            //}
            //await Task.Delay(TimeSpan.FromSeconds(delay));
            //var sensors = ReadDeviceToCloudMessages.Sensors;
            await Task.Delay(1); //Just to make this a proper asnc Task
            return Ok(Sensors);
        }
    }
}
