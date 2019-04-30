using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MessagePush.Context;
using MessagePush.Service;
using MessagePush.Model;
using Microsoft.AspNetCore.Authorization;

namespace MessagePush.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserService userService;
        public UsersController(UserService userService)
        {
            this.userService = userService;
        }

        [Authorize(Roles = "Standard")]
        [HttpGet]
        public async Task<List<User>> GetUsersAsync()
        {
            return await userService.GetUsersAsync();
        }


    }
}