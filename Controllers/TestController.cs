using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
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
    public class TestController : ControllerBase
    {
        private readonly IHubContext<NotificationHub, INotificationHub> hubContext;

        public TestController(IHubContext<NotificationHub, INotificationHub> hubContext)
        {
            this.hubContext = hubContext;
        }

        [HttpGet]
        public string Get()
        {
            return "You are Authorized";
        }

        [HttpGet]
        [Route("BlogAdministrator")]
        [Authorize(Roles = "BlogAdministrator")]
        public string GetBlogAdministrator()
        {            
            return "You are Authorized as Blog Administrator";
        }

        [HttpGet]
        [Route("BlogAdministrator2")]
        public string GetBlogAdministrator2()
        {            
            return User.HasClaim(ClaimTypes.Role, "BlogAdministrator") ?
                User?.Identity?.Name + " is Authorized as Blog Administrator"
                : User?.Identity?.Name + " is NOT a Blog Administrator";
        }

        [HttpGet]
        [Route("SignalRTest")]
        [Authorize(Roles = "BlogAdministrator")]
        public string SendSignalR([FromQuery] string message)
        {
            hubContext.Clients.All.TestMessage(message);
            return "Okay";
        }

        [HttpGet]
        [Route("TestWearOS")]
        [Authorize(Roles = "WearOSApp")]
        public IActionResult TestWearOS()
        {
            return Ok(new
            {
                RequestTimeStamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm.ss"),
                JediCount = 3,
                SithCount = 4
            });
        }
    }
}
