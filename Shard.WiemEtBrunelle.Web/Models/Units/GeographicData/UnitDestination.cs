using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Shard.WiemEtBrunelle.Web.Models.Universe;
using System;

namespace Shard.WiemEtBrunelle.Web.Models.Units.GeographicData
{
    public class UnitDestination
    {
        public StarSystem StarSystem { get; set; }

        public Planet Planet { get; set; }

        [BsonRepresentation(BsonType.String)]
        public DateTime DateOfArrival { get; set; }

        [BsonRepresentation(BsonType.String)]
        public TimeSpan EstimatedTimeOfArrival { get; set; }

        [BsonRepresentation(BsonType.String)]
        public DateTime DateOfEntryInNewSystem { get; set; }
    }
}
