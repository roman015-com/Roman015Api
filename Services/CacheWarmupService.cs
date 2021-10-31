using Microsoft.Extensions.Hosting;
using Roman015API.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Roman015API.Services
{
    public class CacheWarmupService : IHostedService, IDisposable
    {
        private Timer timer;
        private BlogViewerController blogViewerController;

        public CacheWarmupService(BlogViewerController blogViewerController)
        {
            this.blogViewerController = blogViewerController;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            timer = new Timer((state) =>
            {
                blogViewerController.ReloadCache();
            }, 
            null, 
            TimeSpan.Zero,
            TimeSpan.FromHours(3));

            return Task.CompletedTask;
        }        

        public Task StopAsync(CancellationToken cancellationToken)
        {
            timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            timer?.Dispose();
        }
    }
}
