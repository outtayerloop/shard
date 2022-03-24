using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Shard.WiemEtBrunelle.Web.Models.Units
{
    public class Building 
    {
        public Building() { }

        public Building(string id, string type)
        {
            Id = id;
            Type = type;
        }

        [BsonRepresentation(BsonType.String)]
        public DateTime LastMomentOfExtraction { get; set; }

        [BsonId]
        public string Id { get; set; }

        public string Type { get; set; }

        public string ResourceCategory { get; set; }

        public string BuilderId { get; set; }

        public string StarSystem { get; set; }

        public string Planet { get; set; }

        public bool IsBuilt { get; set; }

        [BsonRepresentation(BsonType.String)]
        public DateTime? EstimatedBuildTime { get; set; }

        [BsonRepresentation(BsonType.String)]
        public DateTime BuildCompletionTime { get; set; }

        public List<GenericUnit> UnitsQueue { get; set; }
    }
}
