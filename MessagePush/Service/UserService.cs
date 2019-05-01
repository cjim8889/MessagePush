using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using MessagePush.Context;
using MessagePush.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;

namespace MessagePush.Service
{
    public class UserService
    {
        private readonly IMongoCollection<User> users;
        private readonly IConfiguration configuration;
        public UserService(DatabaseContext databaseContext, IConfiguration configuration)
        {
            users = databaseContext.Database.GetCollection<User>("Users");
            this.configuration = configuration;
        }

        public async Task<List<User>> GetUsersAsync()
        {
            return await users.Find(x => true).ToListAsync();
        }

        public string GenerateJwtToken(User user, DateTime expiryDate)
        {
            var claims = GenerateClaims(user);
                       
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration.GetSection("Jwt:SecretKey").Value));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: configuration.GetValue<string>("Jwt:Issuer"),
                audience: configuration.GetValue<string>("Jwt:Audience"),
                expires: expiryDate,
                claims: claims,
                signingCredentials: creds
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        private static List<Claim> GenerateClaims(User user)
        {
            var claims = new List<Claim>();

            AddRolesToClaims(claims, user.Roles);

            claims.Add(new Claim(ClaimTypes.Email, user.Email));
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));

            return claims;
        }

        private static void AddRolesToClaims(List<Claim> claims, List<string> roles)
        {
            foreach (string role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }
    }
}
