using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roman015API.Hubs
{
    public interface IStarWarsHub
    {
        Task Order66Executed(int JediCount, int SithCount);

        Task JoinSide(bool isJedi);

        Task SwitchSide(bool isJedi);

        Task LeaveSide(bool isJedi);
    }

    public class StarWarsHub : Hub<IStarWarsHub>
    {
        private readonly ILogger<StarWarsHub> logger;
        private readonly IDistributedCache distributedCache;

        private void GetForceCount(out int Jedi, out int Sith)
        {
            string[] counts = Encoding.ASCII.GetString(distributedCache.Get("ForceCount"))
                                .Split(",");
            Jedi = Convert.ToInt32(counts[0]);
            Sith = Convert.ToInt32(counts[1]);
        }

        private void SetForceCount(int Jedi, int Sith)
        {
            string temp = Jedi + "," + Sith;
            distributedCache.Set(
                "ForceCount",
                Encoding.ASCII.GetBytes(temp)
                );
        }

        public static bool IsInitialSetupRequired = false;

        public StarWarsHub(ILogger<StarWarsHub> logger, IDistributedCache distributedCache)
        {
            this.logger = logger;
            this.distributedCache = distributedCache;

            if(distributedCache.Get("ForceCount") == null || IsInitialSetupRequired)
            {
                SetForceCount(0, 0);
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
            int jedi, sith;
            GetForceCount(out jedi, out sith);
            SetForceCount(
                isJedi ? jedi + 1 : jedi,
                !isJedi ? sith + 1 : sith
            );
        }

        public void SwitchSide(bool isJedi)
        {
            int jedi, sith;
            GetForceCount(out jedi, out sith);
            SetForceCount(
                isJedi ? jedi + 1 : jedi - 1,
                !isJedi ? sith + 1 : sith - 1
            );
        }

        public void LeaveSide(bool isJedi)
        {
            int jedi, sith;
            GetForceCount(out jedi, out sith);
            SetForceCount(
                isJedi ? jedi - 1 : jedi,
                !isJedi ? sith - 1 : sith
            );
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
