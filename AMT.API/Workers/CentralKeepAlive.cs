using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Simple.AMT;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AMT.API.Workers
{
    public class CentralKeepAlive : IHostedService, IDisposable
    {
        private readonly AMT8000 central;
        private Timer? _timer;
        public CentralKeepAlive(AMT8000 central)
        {
            this.central = central;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(executaKeepAliveAsync, null, TimeSpan.FromSeconds(15), TimeSpan.FromMinutes(15));
            return Task.CompletedTask;
        }
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        private async void executaKeepAliveAsync(object? state)
        {
            if (!central.IsConnected) await central.ConnectAsync();
            await central.KeepAliveAsync();
        }
    }
}
