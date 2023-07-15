using Microsoft.Extensions.Hosting;
using Simple.AMT;
using System;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Caching.Memory;
using Serilog;

namespace AMT.API.Workers
{
    public class CentralSensorsCacheUpdate : IHostedService, IDisposable
    {
        private readonly AMT8000 central;
        private readonly IMemoryCache memoryCache;
        private readonly ILogger log;
        private Timer? _timer;
        public CentralSensorsCacheUpdate(AMT8000 central, IMemoryCache memoryCache, ILogger log)
        {
            this.central = central;
            this.memoryCache = memoryCache;
            this.log = log;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            lastUpdateNames = DateTime.Now.AddHours(-10);
            lastUpdateCentral = DateTime.Now.AddHours(-1);
            lastKeepAlive = DateTime.Now;
            log.Information("Update task start");
            executaSensorUpdateAsync(null);

            _timer = new Timer(executaSensorUpdateAsync, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(15));
            await Task.CompletedTask;
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

        private async void executaSensorUpdateAsync(object? state)
        {
            if (updating)
            {
                if((DateTime.Now - lastUpdateCentral).TotalMinutes > 5)
                {
                    log.Error("[SensorUpdate] Halt Detection forced a disconnection!");
                    central.Disconnect();
                }
                return;
            }

            if (!central.IsConnected)
            {
                log.Error("[SensorUpdate] Central disconnected!");
                await central.ConnectAsync(); // reconnect
                log.Information("[SensorUpdate] Reconnected");
            }

            updating = true;
            // update Zones
            try
            {
                await Helpers.MemCacheHelper.setCachedZoneStatus(memoryCache, central);
                log.Information("[SensorUpdate] Zone Update");
                await Task.Delay(50);

                if ((DateTime.Now - lastUpdateNames).TotalMinutes > 60)
                {
                    await Helpers.MemCacheHelper.setCachedZoneNames(memoryCache, central);
                    log.Information("[SensorUpdate] Names Update");
                    lastUpdateNames = DateTime.Now;
                    await Task.Delay(50);
                }

                if ((DateTime.Now - lastUpdateCentral).TotalSeconds > 120)
                {
                    await Helpers.MemCacheHelper.setCentralInformation(memoryCache, central);
                    log.Information("[SensorUpdate] Central Update");
                    lastUpdateCentral = DateTime.Now;
                    await Task.Delay(50);
                }

                if ((DateTime.Now - lastKeepAlive).TotalSeconds > 60)
                {
                    await central.KeepAliveAsync();
                    log.Information("[SensorUpdate] KeepAlive");
                    lastKeepAlive = DateTime.Now;
                }
            }
            catch(Exception ex)
            {
                log.Error(ex, "executaSensorUpdateAsync:Err");
                central.Disconnect();
            }

            updating = false;
        }
    }
}
