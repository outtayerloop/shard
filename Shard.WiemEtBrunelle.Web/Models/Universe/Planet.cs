using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using Shard.Shared.Core;
using Shard.WiemEtBrunelle.Web.Models.Units;
using System.Collections.Generic;

namespace Shard.WiemEtBrunelle.Web.Models.Universe
{
    public class Planet
    {
        public Planet(string name, int? size, Dictionary<ResourceKind, int> resourceQuantity)
        {
            Name = name;
            Size = size;
            ResourceQuantity = resourceQuantity;
        }

        public string Name { get; set; }

        public int? Size { get; set; }

        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<ResourceKind, int> ResourceQuantity { get; set; }

        public List<Building> Starports { get; set; }
    }
}