using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Roman015API.Hubs
{
    public interface INotificationHub
    {
        Task TestMessage(string message);
    }

    public class NotificationHub : Hub<INotificationHub>
    {
        private readonly ILogger<NotificationHub> logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            this.logger = logger;
        }

        public async Task SendTestMessage(string message)
        {
            logger.Log(LogLevel.Information, "SendTestMessage");
            try
            {
                await Clients.All.TestMessage(message);
            }
            catch(Exception e)
            {
                logger.Log(LogLevel.Error, "SendTestMessage : " + e.Message);
            }
        }
    }
}
