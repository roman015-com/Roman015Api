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

            if (distributedCache.Get("JediCount") == null)
            {
                distributedCache.Set("JediCount", BitConverter.GetBytes(0));
            }

            if (distributedCache.Get("SithCount") == null)
            {
                distributedCache.Set("SithCount", BitConverter.GetBytes(0));
            }
        }

        [HttpGet]
        [Route("GetCount")]
        [Authorize(Roles = "WearOSApp")]
        public IActionResult GetCount()
        {
            return Ok(new
            {
                RequestTimeStamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm.ss"),
                JediCount = BitConverter.ToInt32(distributedCache.Get("JediCount")),
                SithCount = BitConverter.ToInt32(distributedCache.Get("SithCount"))
            });
        }

        [HttpGet]
        [Route("ExecuteOrder66")]
        [Authorize(Roles = "WearOSApp")]
        public IActionResult ExecuteOrder66()
        {
            hubContext.Clients.All.Order66Executed(
                BitConverter.ToInt32(distributedCache.Get("JediCount")),
                BitConverter.ToInt32(distributedCache.Get("SithCount"))
            );

            return Ok(new
            {
                RequestTimeStamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm.ss"),
                JediCount = BitConverter.ToInt32(distributedCache.Get("JediCount")),
                SithCount = BitConverter.ToInt32(distributedCache.Get("SithCount"))
            });
        }
    }
}
