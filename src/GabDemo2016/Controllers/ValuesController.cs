using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.ApplicationInsights;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Mvc;

namespace GabDemo2016.Controllers
{    
    public class ValuesController : Controller
    {
        private readonly IHostingEnvironment _env;

        public ValuesController(IHostingEnvironment env)
        {
            _env = env;
        }

        [HttpGet, Route("api/values")]
        public IEnumerable<string> Get()
        {
            Trace.TraceInformation("Values requested.");

            return new[] { "value1", "value2", _env.EnvironmentName, _env.IsDevelopment().ToString() };
        }

        [HttpGet, Route("api/values/error")]
        public IActionResult Error()
        {
            throw new Exception("Generated Exception for testing");
        }


        [HttpGet, Route("api/values/{id}")]
        public IActionResult Get(int id)
        {
            Trace.TraceInformation($"Requested value: {id}");
            if (id <= 0)
            {
                return new BadRequestObjectResult("ID must be larger than 0");
            }

            return new ObjectResult("value");
        }

        [HttpGet, Route("api/values/metrics")]
        public IActionResult CustomMetrics()
        {
            var tc = new TelemetryClient();
            // Set up some properties:
            var properties = new Dictionary<string, string> { { "Game", "GameName" }, { "Difficulty", "Hard" } };
            var measurements = new Dictionary<string, double> { { "GameScore", 20 }, { "Opponents", 1 } };
            tc.TrackEvent("WinGame", properties, measurements);
            tc.TrackMetric("GameScore", 20, properties);

            return Ok("Metrics added");
        }

        [HttpPost, Route("api/values")]
        public IActionResult Post([FromBody]string value)
        {
            return Ok(value);
        }
    }
}
