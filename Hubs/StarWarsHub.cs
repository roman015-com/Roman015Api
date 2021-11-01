using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Roman015API.Hubs
{
    public interface IStarWarsHub
    {
        Task Order66Executed(int JediCount, int SithCount);

        Task JoinSide(bool isJedi);
    }

    public class StarWarsHub : Hub<IStarWarsHub>
    {
        private readonly ILogger<StarWarsHub> logger;
        private readonly IDistributedCache distributedCache;

        public StarWarsHub(ILogger<StarWarsHub> logger, IDistributedCache distributedCache)
        {
            this.logger = logger;
            this.distributedCache = distributedCache;

            if(distributedCache.Get("JediCount") == null)
            {
                distributedCache.Set("JediCount", BitConverter.GetBytes(0));
            }

            if (distributedCache.Get("SithCount") == null)
            {
                distributedCache.Set("SithCount", BitConverter.GetBytes(0));
            }
        }

        public async Task Order66Executed(int JediCount, int SithCount)
        {
            logger.Log(LogLevel.Information, "Order66Executed");
            try
            {
                await Clients.All.Order66Executed(JediCount, SithCount);
            }
            catch(Exception e)
            {
                logger.Log(LogLevel.Error, "Order66Executed : " + e.Message);
            }
        }

        public async Task JoinSide(bool isJedi)
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                isJedi ? "Jedi" : "Sith");
            
            if(isJedi)
            {                
                distributedCache.Set(
                    "JediCount",
                    BitConverter.GetBytes(
                        BitConverter.ToInt32(distributedCache.Get("JediCount")) + 1)
                );
            }
            else
            {
                distributedCache.Set(
                    "SithCount",
                    BitConverter.GetBytes(
                        BitConverter.ToInt32(distributedCache.Get("SithCount")) + 1)
                );
            }
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            logger.Log(LogLevel.Information, "OnDisconnectedAsync : ConnectionId " + Context.ConnectionId);
            try
            {
                Groups.RemoveFromGroupAsync(Context.ConnectionId, "JediCount");
                Groups.RemoveFromGroupAsync(Context.ConnectionId, "SithCount");
            }
            catch (Exception e)
            {
                logger.Log(LogLevel.Error, "OnDisconnectedAsync : " + e.Message);
            }

            return base.OnDisconnectedAsync(exception);
        }
    }
}
