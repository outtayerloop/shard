using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using Shard.Shared.Core;
using Shard.WiemEtBrunelle.Web.Dto.Users;
using Shard.WiemEtBrunelle.Web.Models.Units;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Shard.WiemEtBrunelle.Web.Models.Users
{
    public class User
    {
        
        public User(string id) {
            Id = id;
        }

        public User(string id, DateTimeOffset dateOfCreation, Dictionary<ResourceKind, int> resourcesQuantity)
        {
            Id = id;
            DateOfCreation = dateOfCreation;
            ResourcesQuantity = resourcesQuantity;
        }

        [BsonId]
        public string Id { get; set; }

        public string Pseudo { get; set; }


        [BsonRepresentation(BsonType.String)]
        public DateTimeOffset DateOfCreation { get; set; }


        public List<GenericUnit> Units { get; set; }


        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<ResourceKind, int> ResourcesQuantity { get; set; }


        public async Task<UserDto> TeleportToAnotherShard(UserDto userToTeleport, HttpClient configuredClient, string baseUri)
        {
            string fullTeleportUri = $"{baseUri}/users/{userToTeleport.Id}";
            var response = await configuredClient.PutAsJsonAsync(fullTeleportUri, userToTeleport);
            return response.IsSuccessStatusCode
                ? userToTeleport
                : null;
        }
    }
}
