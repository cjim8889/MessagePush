using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
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
            Roles = new List<string>() { Role.Standard };
            RegisteredAt = DateTime.Now;

            using (RandomNumberGenerator rng = new RNGCryptoServiceProvider())
            {
                byte[] adminTokenData = new byte[32];
                byte[] pushTokenData = new byte[32];

                rng.GetBytes(adminTokenData);
                rng.GetBytes(pushTokenData);

                AdminToken = WebEncoders.Base64UrlEncode(adminTokenData);
                PushToken = WebEncoders.Base64UrlEncode(pushTokenData);
            }
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
        public List<string> Roles { get; set; }
    }
}
