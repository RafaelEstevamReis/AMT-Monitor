using Microsoft.Extensions.Hosting;
using Simple.AMT;
using System;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Caching.Memory;

namespace AMT.API.Workers
{
    public class CentralSensorsCacheUpdate : IHostedService, IDisposable
    {
        private readonly AMT8000 central;
        private readonly IMemoryCache memoryCache;
        private Timer? _timer;
        public CentralSensorsCacheUpdate(AMT8000 central, IMemoryCache memoryCache)
        {
            this.central = central;
            this.memoryCache = memoryCache;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(executaKeepAliveAsync, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10));
            lastUpdateNames = DateTime.Now.AddHours(-10);
            lastUpdateCentral = DateTime.Now.AddHours(-1);
            lastKeepAlive = DateTime.Now;
            return Task.CompletedTask;
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public DateTime lastUpdateNames;
        public DateTime lastUpdateCentral;
        public DateTime lastKeepAlive;
        bool updating = false;

        private async void executaKeepAliveAsync(object? state)
        {
            if (updating) return;
            updating = true;
            // update Zones
            await Helpers.MemCacheHelper.setCachedZoneStatus(memoryCache, central);
            await Task.Delay(50);

            if ((DateTime.Now - lastUpdateNames).TotalMinutes > 60)
            {
                await Helpers.MemCacheHelper.setCachedZoneNames(memoryCache, central);
                lastUpdateNames = DateTime.Now;
                await Task.Delay(50);
            }
            if ((DateTime.Now - lastUpdateCentral).TotalSeconds > 60)
            {
                await Helpers.MemCacheHelper.setCentralInformation(memoryCache, central);
                lastUpdateCentral = DateTime.Now;
                await Task.Delay(50);
            }

            if ((DateTime.Now - lastKeepAlive).TotalSeconds > 60)
            {
                await central.KeepAliveAsync();
                lastUpdateCentral = DateTime.Now;
            }
            updating = false;
        }
    }
}
