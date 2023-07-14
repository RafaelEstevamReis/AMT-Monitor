using Microsoft.Extensions.Caching.Memory;
using Simple.AMT;
using System;
using System.Threading.Tasks;

namespace AMT.API.Helpers
{
    internal static class MemCacheHelper
    {
        private const string CACHEKEY_CENTRAL_INFORMATION = "Central.Information";
        private const string CACHEKEY_CENTRAL_ZONESNAMES = "Central.ZonesNames";
        private const string CACHEKEY_CENTRAL_OPENEDZONES = "Central.OpenedZones";

        internal static async Task setCentralInformation(IMemoryCache memoryCache, AMT8000 central)
        {
            memoryCache.Set(CACHEKEY_CENTRAL_INFORMATION, await central.GetCentralStatusAsync(), TimeSpan.FromMinutes(5));
        }
        internal static Simple.AMT.AMTPackets.CentralStatus getCentralInformation(IMemoryCache memoryCache)
        {
            return memoryCache.Get<Simple.AMT.AMTPackets.CentralStatus>(CACHEKEY_CENTRAL_INFORMATION);
        }

        internal static async Task setCachedZoneNames(IMemoryCache memoryCache, AMT8000 central)
        {
            memoryCache.Set(CACHEKEY_CENTRAL_ZONESNAMES, await central.GetZonesNamesAsync(), TimeSpan.FromMinutes(5));
        }
        internal static Simple.AMT.AMTPackets.ItemNames.NameEntry[] getCachedZoneNames(IMemoryCache memoryCache)
        {
            return memoryCache.Get<Simple.AMT.AMTPackets.ItemNames.NameEntry[]>(CACHEKEY_CENTRAL_ZONESNAMES);
        }

        internal static async Task setCachedZoneStatus(IMemoryCache memoryCache, AMT8000 central)
        {
            memoryCache.Set(CACHEKEY_CENTRAL_OPENEDZONES, await central.GetOpenedZonesAsync(), TimeSpan.FromMinutes(1));
        }
        internal static bool[] getCachedZoneStatus(IMemoryCache memoryCache)
        {
            return memoryCache.Get<bool[]>(CACHEKEY_CENTRAL_OPENEDZONES);
        }

    }
}
