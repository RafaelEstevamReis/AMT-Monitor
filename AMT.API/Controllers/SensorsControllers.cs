using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Simple.AMT;
using System.Threading.Tasks;

namespace AMT.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SensorsControllers : ControllerBase
    {
        private readonly ILogger<SensorsControllers> logger;
        private readonly AMT8000 central;

        public SensorsControllers(ILogger<SensorsControllers> logger, AMT8000 central)
        {
            this.logger = logger;
            this.central = central;
        }


        [HttpGet]
        [Route("GetSensorConfiguration")]
        public async Task<ActionResult> GetSensorConfigurationAsync()
        {
            var result = await central.GetSensorConfigurationAsync();
            return Ok(result);
        }
    }
}
