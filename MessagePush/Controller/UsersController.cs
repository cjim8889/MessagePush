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
using Microsoft.AspNetCore.Cors;

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

        public class AddRoles
        {
            public string[] Roles { get; set; }
        }

        public class RemoveRoles
        {
            public string[] Roles { get; set; }
        }

        private readonly UserService userService;
        public UsersController(UserService userService)
        {
            this.userService = userService;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<List<User>> GetUsersAsync()
        {
            return await userService.GetUsersAsync();
        }

        //[Authorize(Roles = "Admin")]
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

        [Authorize]
        [HttpGet("user")]
        public async Task<ActionResult<User>> GetUserByClaim()
        {
            var id = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

            return Ok(await userService.GetUserByIdAsync(id));
        }

        [Authorize]
        [HttpGet("user/token/refresh")]
        public async Task<ActionResult> RefreshUserToken()
        {
            var id = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;

            var tokens = await userService.RefreshUserToken(id);

            return Ok(new { tokens.AdminToken, tokens.PushToken });
        }

        //[Authorize(Roles = "Admin")]
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

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}/roles")]
        public async Task<ActionResult> GetUserRolesById(string id)
        {
            var user = await userService.GetUserByIdAsync(id);
            if (user != null)
            {
                return Ok(user.Roles);
            }

            return NotFound();
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/roles/{role}")]
        public async Task<ActionResult> AddRoleToUser(string id, string role)
        {
            var result = await userService.AddRoleToUserAsync(id, role);

            return result ? Ok() : (ActionResult)BadRequest();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{id}/roles")]
        public async Task<ActionResult> AddRolesToUser(string id, [FromBody] AddRoles addRolesPost)
        {
            var result = await userService.AddRolesToUserAsync(id, addRolesPost.Roles);

            return result ? Ok() : (ActionResult)BadRequest();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}/roles/{role}")]
        public async Task<ActionResult> RemoveRoleOfUser(string id, string role)
        {
            var result = await userService.RemoveRoleOfUserAsync(id, role);

            return result ? Ok() : (ActionResult)BadRequest();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}/roles")]
        public async Task<ActionResult> RemoveRoleOfUser(string id, [FromBody] RemoveRoles roles)
        {
            var result = await userService.RemoveRolesOfUserAsync(id, roles.Roles);

            return result ? Ok() : (ActionResult)BadRequest();
        }


        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> RemoveUserById(string id)
        {
            await userService.RemoveUserByIdAsync(id);
            return NoContent();
        }

        [Authorize(Roles = "Standard")]
        [HttpDelete("user")]
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

            return user != null
                ? Ok(new ReturnMessage() { StatusCode = 1, Message = userService.GenerateJwtToken(user, DateTime.Now.AddHours(1))})
                : (ActionResult)BadRequest(new ReturnMessage() { StatusCode = 2, Message = "Invalid Admin Token" });
        }

        //[EnableCors("AllowAny")]
        [AllowAnonymous]
        [HttpGet("login")]
        public async Task<ActionResult> LogIn([FromQuery] string email, [FromQuery] string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return BadRequest();
            }

            var user = await userService.GetUserByEmailAndPasswordAsync(email, password);

            return user != null
                ? Ok(new ReturnMessage() { StatusCode = 1, Message = userService.GenerateJwtToken(user, DateTime.Now.AddHours(1)) })
                : (ActionResult)BadRequest(new ReturnMessage() { StatusCode = 2, Message = "Invalid Email Or Password" });
        }
    }
}