using MongoDB.Bson.Serialization.Attributes;
using Shard.WiemEtBrunelle.Web.Models.Universe;

namespace Shard.WiemEtBrunelle.Web.Models.Units.GeographicData
{
    [BsonDiscriminator(RootClass = true)]
    [BsonKnownTypes(typeof(ResourcelessUnitLocation), typeof(UnitLocationWithResources))]
    public abstract class BaseUnitLocation
    {
        public Planet Planet { get; set; }

        public StarSystem StarSystem { get; set; }

        public abstract bool HasResources();
    }
}