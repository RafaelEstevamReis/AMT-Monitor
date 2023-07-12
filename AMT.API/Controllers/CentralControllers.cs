using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Simple.AMT;
using System.Threading.Tasks;

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

        [HttpGet]
        [Route("GetMAC")]
        public async Task<ActionResult> GetMAC()
        {
            return Ok(await central.GetMacAsync());
        }

        [HttpGet]
        [Route("GetFullStatus")]
        public async Task<ActionResult> GetFullStatus()
        {
            var status = await central.GetCentralStatusAsync();
            status.Header = null;
            status.Data = null;
            return Ok(status);
        }

        [HttpGet]
        [Route("GetConnections")]
        public async Task<ActionResult> GetConnectionsAsync()
        {
            var status = await central.GetConnectionsAsync();
            status.Header = null;
            status.Data = null;
            return Ok(status);
        }
    }
}
