using Microsoft.Extensions.Caching.Memory;
using Simple.AMT;
using System;
using System.Threading.Tasks;

namespace AMT.API.Helpers
{
    internal static class MemCacheHelper
    {
        internal static async Task<Simple.AMT.AMTPackets.CentralStatus> getCentralInformation(IMemoryCache memoryCache, AMT8000 central)
        {
            return await memoryCache.GetOrCreate(
                $"Central.Information",
                async cacheEntry =>
                {
                    cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
                    return await central.GetCentralStatusAsync();
                });
        }
        internal static async Task<Simple.AMT.AMTPackets.ItemNames.NameEntry[]> getCachedZoneNames(IMemoryCache memoryCache, AMT8000 central)
        {
            return await memoryCache.GetOrCreate(
                $"Central.ZonesNames",
                async cacheEntry =>
                {
                    cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                    return await central.GetZonesNamesAsync();
                });
        }
        internal static async Task<bool[]> getCachedZoneStatus(IMemoryCache memoryCache, AMT8000 central, bool forceUpdate)
        {
            if (forceUpdate)
            {
                memoryCache.Remove("Central.OpenedZones");
            }

            return await memoryCache.GetOrCreate(
                "Central.OpenedZones",
                async cacheEntry =>
                {
                    cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60);
                    return await central.GetOpenedZonesAsync();
                });
        }


    }
}
