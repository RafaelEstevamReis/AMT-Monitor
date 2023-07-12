using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Simple.AMT;

namespace AMT.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CentralControllers : ControllerBase
    {
        private readonly ILogger<CentralControllers> logger;
        private readonly AMT8000 central;

        public CentralControllers(ILogger<CentralControllers> logger, AMT8000 central)
        {
            this.logger = logger;
            this.central = central;
        }


    }
}
