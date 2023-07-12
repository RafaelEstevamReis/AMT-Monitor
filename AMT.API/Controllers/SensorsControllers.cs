using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Simple.AMT;

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


    }
}
