using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MessagePush.Context;
using MessagePush.Model;
using MongoDB.Driver;

namespace MessagePush.Service
{
    public class UserService
    {
        private readonly IMongoCollection<User> users;
        public UserService(DatabaseContext databaseContext)
        {
            users = databaseContext.Database.GetCollection<User>("Users");
        }

        public async Task<List<User>> GetUsersAsync()
        {
            return await users.Find(x => true).ToListAsync();
        }
    }
}
