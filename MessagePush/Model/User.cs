﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MessagePush.Service;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MessagePush.Model
{
    public static class Role
    {
        public const string Admin = "Admin";
        public const string Privileged = "Privileged";
        public const string Standard = "Standard";
    }
    public class User
    {
        public User()
        {
            Validated = false;
            Subscribers = new List<int>();
            Roles = new HashSet<string>() { Role.Standard };
            RegisteredAt = DateTime.Now;

            AdminToken = UserService.GenerateToken();
            PushToken = UserService.GenerateToken();
        }


        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("PushToken")]
        public string PushToken { get; set; }

        [BsonElement("AdminToken")]
        public string AdminToken { get; set; }

        [BsonElement("Password")]
        public string Password { get; set; }

        [BsonElement("Email")]
        public string Email { get; set; }

        [BsonElement("Subscribers")]
        public List<int> Subscribers { get; set; }

        [BsonElement("Validated")]
        public bool Validated { get; set; }

        [BsonElement("RegisteredAt")]
        public DateTime RegisteredAt { get; set; }
        [BsonElement("Roles")]
        public HashSet<string> Roles { get; set; }
    }
}
