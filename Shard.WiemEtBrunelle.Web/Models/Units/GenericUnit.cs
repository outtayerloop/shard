using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using Shard.Shared.Core;
using Shard.WiemEtBrunelle.Web.Dto.Units;
using Shard.WiemEtBrunelle.Web.Models.Units.Battle;
using Shard.WiemEtBrunelle.Web.Models.Units.GeographicData;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Shard.WiemEtBrunelle.Web.Models.Units
{
    [BsonDiscriminator(RootClass = true)]
    [BsonKnownTypes(typeof(ScoutUnit), typeof(CargoUnit), typeof(BuilderUnit), typeof(BaseBattleUnit))]
    public class GenericUnit
    {

        public GenericUnit(string id)
        {
            Id = id;
        }

        public GenericUnit() { }

        [BsonId]
        public string Id { get; set; }

        public BaseUnitLocation UnitLocation { get; set; }

        public UnitDestination UnitDestination { get; set; }

        public virtual string UnitType => null;

        public virtual bool CanScanPlanetResources => false;

        public virtual int Health { get; set; }

        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<ResourceKind, int> ResourcesQuantity { get; set; }

        public void ReachDestination()
        {
            UnitLocation.StarSystem = UnitDestination.StarSystem;
            UnitLocation.Planet = UnitDestination.Planet;
            UnitDestination.EstimatedTimeOfArrival = new TimeSpan(0, 0, 0);
        }

        public bool UnitHasReachedDestination()
            => UnitDestination.EstimatedTimeOfArrival.Minutes == 0 && UnitDestination.EstimatedTimeOfArrival.Seconds == 0;

        public async Task<(UnitDto, string)> TeleportToAnotherShard(string userId, UnitDto unitToTeleport, 
            HttpClient configuredClient, string baseUri)
        {
            string fullTeleportUri = $"{baseUri}/users/{userId}/units/{unitToTeleport.Id}";
            var response = await configuredClient.PutAsJsonAsync(fullTeleportUri, unitToTeleport);
            return response.IsSuccessStatusCode
                ? (unitToTeleport, fullTeleportUri)
                : (null, null);
        }

    }

}