using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Simple.AMT;

namespace AMT.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BasicController : ControllerBase
    {
        private readonly ILogger<BasicController> logger;
        private readonly AMT8000 central;

        public BasicController(ILogger<BasicController> logger, AMT8000 central)
        {
            this.logger = logger;
            this.central = central;
        }

        [HttpGet]
        [Route("IsConnected")]
        public ActionResult IsConnected()
        {
            return Ok(central.IsConnected);
        }
    }
}
