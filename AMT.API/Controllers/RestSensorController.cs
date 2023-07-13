using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Simple.AMT;
using System.Threading.Tasks;
using System;

namespace AMT.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RestSensorController : ControllerBase
    {
        private readonly ILogger<RestSensorController> logger;
        private readonly AMT8000 central;
        private readonly IMemoryCache memoryCache;

        public RestSensorController(ILogger<RestSensorController> logger, AMT8000 central, IMemoryCache memoryCache)
        {
            this.logger = logger;
            this.central = central;
            this.memoryCache = memoryCache;
        }

        [HttpGet]
        [Route("GetCentralArmedStatus")]
        public async Task<RestSensorArmedModel> GetCentralArmedStatusAsync()
        {
            var cachedValue = await Helpers.MemCacheHelper.getCentralInformation(memoryCache, central);

            return new RestSensorArmedModel
            {
                Armed = cachedValue.Status == 2 ? "true" : "false",
            };
        }
        [HttpGet]
        [Route("GetCentralAllClosedStatus")]
        public async Task<RestSensorValueModel> GetCentralAllClosedStatusAsync()
        {
            var cachedValue = await Helpers.MemCacheHelper.getCentralInformation(memoryCache, central);

            return new RestSensorValueModel
            {
                Value = cachedValue.AllZonesClosed ? "true" : "false",
            };
        }

        [HttpGet]
        [Route("GetSensorStatus/{zoneId}")]
        public async Task<RestOpenSensorModel> GetSensorStatusAsync(int zoneId)
        {
            if (zoneId < 1 || zoneId > 64) throw new ArgumentOutOfRangeException("zone Id must be between 1 and 64");

            int zoneIndex = zoneId - 1;
            // use memoryCache
            var cachedNames = await Helpers.MemCacheHelper.getCachedZoneNames(memoryCache, central);
            var cachedValue = await Helpers.MemCacheHelper.getCachedZoneStatus(memoryCache, central, forceUpdate: false);

            return new RestOpenSensorModel()
            {
                Name = cachedNames[zoneIndex].Name,
                State = new RestOpenSensorStateModel()
                {
                    Open = cachedValue[zoneIndex] ? "true" : "false",
                    Timestamp = DateTime.UtcNow,
                }
            };
        }

    }
    public class RestOpenSensorModel
    {
        public string Name { get; set; }
        public RestOpenSensorStateModel State { get; set; }
    }
    public class RestOpenSensorStateModel
    {
        public string Open { get; set; }
        public DateTime Timestamp { get; set; }
    }
    public class RestSensorValueModel
    {
        public string Value { get; set; }
    }
    public class RestSensorArmedModel
    {
        public string Armed { get; set; }
    }
}
