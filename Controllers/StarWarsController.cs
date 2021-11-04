using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using Roman015API.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Roman015API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class StarWarsController : ControllerBase
    {
        private readonly IHubContext<StarWarsHub, IStarWarsHub> hubContext;
        private readonly IDistributedCache distributedCache;

        public StarWarsController(IHubContext<StarWarsHub, IStarWarsHub> hubContext, IDistributedCache distributedCache)
        {
            this.hubContext = hubContext;
            this.distributedCache = distributedCache;

            if (distributedCache.Get("ForceCount") == null)
            {
                SetForceCount(0, 0);
            }

            if (distributedCache.Get("TotalCount") == null)
            {
                distributedCache.Set("TotalCount", BitConverter.GetBytes(0));
            }
        }

        [HttpGet]
        [Route("GetCount")]
        [Authorize(Roles = "WearOSApp")]
        public IActionResult GetCount()
        {
            GetForceCount(out int jediCount, out int sithCount);
            return Ok(new
            {
                RequestTimeStamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm.ss"),
                JediCount = jediCount,
                SithCount = sithCount,
                TotalCount = BitConverter.ToInt32(distributedCache.Get("TotalCount"))
            });
        }

        [HttpGet]
        [Route("ExecuteOrder66")]
        [Authorize(Roles = "WearOSApp")]
        public IActionResult ExecuteOrder66()
        {
            GetForceCount(out int jediCount, out int sithCount);
            int totalCount = BitConverter.ToInt32(distributedCache.Get("TotalCount"));

            SetForceCount(0, 0);

            hubContext.Clients.All.Order66Executed(jediCount, sithCount);

            return Ok(new
            {
                RequestTimeStamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm.ss"),
                JediCount = jediCount,
                SithCount = sithCount,
                TotalCount = totalCount
            });
        }

        public void GetForceCount(out int Jedi, out int Sith)
        {
            string[] counts = Encoding.ASCII.GetString(distributedCache.Get("ForceCount"))
                                .Split(",");
            Jedi = Convert.ToInt32(counts[0]);
            Sith = Convert.ToInt32(counts[1]);
        }

        public void SetForceCount(int Jedi, int Sith)
        {
            string temp = Jedi + "," + Sith;
            distributedCache.Set(
                "ForceCount",
                Encoding.ASCII.GetBytes(temp)
                );
        }
    }
}
