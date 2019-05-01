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
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MessagePush.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        public class UserDTO
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        private readonly UserService userService;
        public UsersController(UserService userService)
        {
            this.userService = userService;
        }

        //[Authorize(Roles = "Admin")]
        [AllowAnonymous]
        [HttpGet]
        public async Task<List<User>> GetUsersAsync()
        {
            return await userService.GetUsersAsync();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<ActionResult> CreateUser(UserDTO userDTO)
        {
            var user = new User() { Email = userDTO.Email, Password = userDTO.Password };

            if (!userService.ValidateUserData(user))
            {
                return BadRequest(new ReturnMessage() { StatusCode = 665, Message = "Invalid Email Or Password" });
            }

            if (await userService.IsEmailExistsAsync(user.Email))
            {
                return BadRequest(new ReturnMessage() { StatusCode = 666, Message = "Duplicate email address" });
            }


            await userService.CreateUserAsync(user);
            return CreatedAtAction("CreateUser", user);
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUserById(string id)
        {
            var user = await userService.GetUserByIdAsync(id);
            if (user != null)
            {
                return Ok(user);
            }

            return NotFound();
        }

        [AllowAnonymous]
        [HttpDelete("{id}")]
        public async Task<ActionResult> RemoveUserById(string id)
        {
            await userService.RemoveUserByIdAsync(id);
            return NoContent();
        }

        [Authorize(Roles = "Standard")]
        [HttpDelete("self")]
        public async Task<ActionResult> RemoveUserByUser()
        {
            var id = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

            if (!string.IsNullOrWhiteSpace(id))
            {
                await userService.RemoveUserByIdAsync(id);
                return NoContent();
            }

            return BadRequest();
        }

        [AllowAnonymous]
        [HttpGet("login/{adminToken}")]
        public async Task<ActionResult> LogInByAdminToken(string adminToken)
        {
            var user = await userService.GetUserByAdminTokenAsync(adminToken);

            if (user != null)
            {
                return Ok(new ReturnMessage() { StatusCode = 1, Message = userService.GenerateJwtToken(user, DateTime.Now.AddHours(1))});
            }

            return BadRequest(new ReturnMessage() { StatusCode = 2, Message = "Invalid Admin Token" });
        }

        [AllowAnonymous]
        [HttpGet("login")]
        public async Task<ActionResult> LogIn([FromQuery] string email, [FromQuery] string password)
        {
            var user = await userService.GetUserByEmailAndPasswordAsync(email, password);

            if (user != null)
            {
                return Ok(new ReturnMessage() { StatusCode = 1, Message = userService.GenerateJwtToken(user, DateTime.Now.AddHours(1)) });
            }

            return BadRequest(new ReturnMessage() { StatusCode = 2, Message = "Invalid Email Or Password" });
        }
    }
}