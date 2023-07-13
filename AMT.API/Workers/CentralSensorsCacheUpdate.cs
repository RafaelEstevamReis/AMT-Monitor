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
            _timer = new Timer(executaKeepAliveAsync, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(20));
            return Task.CompletedTask;
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        private async void executaKeepAliveAsync(object? state)
        {
            await Helpers.MemCacheHelper.getCachedZoneStatus(memoryCache, central, forceUpdate: true);
        }
    }
}
