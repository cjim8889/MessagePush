﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MessagePush.Context;
using MessagePush.Model;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
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

        public async Task CreateUserAsync(User user)
        {
            user.Password = HashString(user.Password);

            await users.InsertOneAsync(user);
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

        public string HashString(string str)
        {
            byte[] salt = Encoding.ASCII.GetBytes(configuration.GetSection("SecretKey").Value);

            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: str,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 100,
                numBytesRequested: 256 / 8
                ));
        }

        private static List<Claim> GenerateClaims(User user)
        {
            var claims = new List<Claim>();

            AddRolesToClaims(claims, user.Roles);

            claims.Add(new Claim(ClaimTypes.Email, user.Email));
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));

            return claims;
        }

        public async Task RemoveUserByIdAsync(string id)
        {
            await users.DeleteOneAsync(x => x.Id == id);
        }

        public async Task<User> GetUserByIdAsync(string id)
        {
            return await users.Find(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task<User> GetUserByAdminTokenAsync(string adminToken)
        {
            return await users.Find(x => x.AdminToken == adminToken).FirstOrDefaultAsync();
        }

        public async Task<User> GetUserByEmailAndPasswordAsync(string email, string password)
        {
            return await users.Find(x => x.Email == email & x.Password == HashString(password)).FirstOrDefaultAsync();
        }

        public async Task<bool> IsEmailExistsAsync(string email)
        {
            var user = await users.Find(x => x.Email == email).FirstOrDefaultAsync();

            return user != null;
        }

        public bool ValidateUserData(User user)
        {
            return ValidatePassword(user) & ValidateEmail(user);
        }

        private bool ValidateEmail(User user)
        {
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(user.Email,
                    @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                    @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }
        private bool ValidatePassword(User user)
        {
            if (string.IsNullOrWhiteSpace(user.Password) || user.Password.Length < 8)
            {
                return false;
            }
            else if (user.Password.Length > 64)
            {
                return false;
            }

            return true;
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
