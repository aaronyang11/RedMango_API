using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RedMango_API.Utility;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RedMango_API.Controllers
{
    [Route("api/AuthTest")]
    public class AuthTestController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<string>> GetSomething()
        {
            return "You are authenticated.";
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = SD.Role_Admin)]
        public async Task<ActionResult<string>> GetSomething(int someValue)
        {
            return "You are authorized with the role of admin";
        }
    }
}

