using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Simple.AMT;
using System;
using System.Threading.Tasks;

namespace AMT.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ZonesControllers : ControllerBase
    {
        private readonly ILogger<ZonesControllers> logger;
        private readonly AMT8000 central;
        private readonly IMemoryCache memoryCache;

        public ZonesControllers(ILogger<ZonesControllers> logger, AMT8000 central, IMemoryCache memoryCache)
        {
            this.logger = logger;
            this.central = central;
            this.memoryCache = memoryCache;
        }

        [HttpGet]
        [Route("GetZonesNamesAsync")]
        public async Task<ActionResult> GetZonesNamesAsync()
        {
            return Ok(await central.GetZonesNamesAsync());
        }

        [HttpGet]
        [Route("GetOpenedZonesAsync")]
        public async Task<ActionResult> GetOpenedZonesAsync()
        {
            return Ok(await central.GetOpenedZonesAsync());
        }
        [HttpGet]
        [Route("GetBypassedZonesAsync")]
        public async Task<ActionResult> GetBypassedZonesAsync()
        {
            return Ok(await central.GetBypassedZonesAsync());
        }
        [HttpGet]
        [Route("GetTriggeredZonesAsync")]
        public async Task<ActionResult> GetTriggeredZonesAsync()
        {
            return Ok(await central.GetTriggeredZonesAsync());
        }

        [HttpGet]
        [Route("GetZoneOpenStatus/{zoneId}")]
        public async Task<bool> GetZoneOpenStatus(int zoneId)
        {
            if (zoneId < 1 || zoneId > 64) throw new ArgumentOutOfRangeException("zone Id must be between 1 and 64");

            int zoneIndex = zoneId--;
            // use memoryCache
            var cachedValue = await Helpers.MemCacheHelper.getCachedZoneStatus(memoryCache, central, forceUpdate: false);

            return cachedValue[zoneIndex];
        }

    }
}
