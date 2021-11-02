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

        void JoinSide(bool isJedi);

        void SwitchSide(bool isJedi);

        void LeaveSide(bool isJedi);
    }

    public class StarWarsHub : Hub<IStarWarsHub>
    {
        private readonly ILogger<StarWarsHub> logger;
        private readonly IDistributedCache distributedCache;

        public static bool IsInitialSetupRequired = false;

        public StarWarsHub(ILogger<StarWarsHub> logger, IDistributedCache distributedCache)
        {
            this.logger = logger;
            this.distributedCache = distributedCache;

            if(distributedCache.Get("JediLis") == null || IsInitialSetupRequired)
            {
                distributedCache.Set("JediCount", BitConverter.GetBytes(0));
            }

            if (distributedCache.Get("SithCount") == null || IsInitialSetupRequired)
            {
                distributedCache.Set("SithCount", BitConverter.GetBytes(0));
            }

            if(distributedCache.Get("TotalCount") == null || IsInitialSetupRequired)
            {
                distributedCache.Set("TotalCount", BitConverter.GetBytes(0));
            }

            IsInitialSetupRequired = false;
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

        public void JoinSide(bool isJedi)
        {
            //await Groups.AddToGroupAsync(
            //    Context.ConnectionId,
            //    isJedi ? "Jedi" : "Sith");

            if (isJedi)
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

        public void SwitchSide(bool isJedi)
        {
            //await Groups.RemoveFromGroupAsync(
            //    Context.ConnectionId,
            //    !isJedi ? "Jedi" : "Sith");

            //await Groups.AddToGroupAsync(
            //    Context.ConnectionId,
            //    isJedi ? "Jedi" : "Sith");

            if (isJedi)
            {
                distributedCache.Set(
                    "JediCount",
                    BitConverter.GetBytes(
                        BitConverter.ToInt32(distributedCache.Get("JediCount")) + 1)
                );
                distributedCache.Set(
                    "SithCount",
                    BitConverter.GetBytes(
                        BitConverter.ToInt32(distributedCache.Get("SithCount")) - 1)
                );
            }
            else
            {
                distributedCache.Set(
                    "JediCount",
                    BitConverter.GetBytes(
                        BitConverter.ToInt32(distributedCache.Get("JediCount")) - 1)
                );
                distributedCache.Set(
                    "SithCount",
                    BitConverter.GetBytes(
                        BitConverter.ToInt32(distributedCache.Get("SithCount")) + 1)
                );
            }
        }

        public void LeaveSide(bool isJedi)
        {
            //await Groups.RemoveFromGroupAsync(
            //    Context.ConnectionId,
            //    isJedi ? "Jedi" : "Sith");

            if (isJedi)
            {
                distributedCache.Set(
                    "JediCount",
                    BitConverter.GetBytes(
                        BitConverter.ToInt32(distributedCache.Get("JediCount")) - 1)
                );
            }
            else
            {
                distributedCache.Set(
                    "SithCount",
                    BitConverter.GetBytes(
                        BitConverter.ToInt32(distributedCache.Get("SithCount")) - 1)
                );
            }
        }

        public override Task OnConnectedAsync()
        {
            distributedCache.Set("TotalCount", BitConverter.GetBytes(
                BitConverter.ToInt32(distributedCache.Get("TotalCount")) + 1
                ));

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            logger.Log(LogLevel.Warning, "OnDisconnectedAsync : ConnectionId " + Context.ConnectionId);

            distributedCache.Set("TotalCount", BitConverter.GetBytes(
                BitConverter.ToInt32(distributedCache.Get("TotalCount")) - 1
                ));

            return base.OnDisconnectedAsync(exception);
        }
    }
}
