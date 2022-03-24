using MongoDB.Driver;
using Shard.WiemEtBrunelle.Web.Database.Services;
using Shard.WiemEtBrunelle.Web.Models.Units;
using System.Collections.Generic;
using System.Linq;

namespace Shard.WiemEtBrunelle.Web.Repositories.Buildings
{
    public class BuildingRepository : IBuildingRepository
    {

        private readonly IMongoCollection<Building> _buildingCollection;

        public BuildingRepository(MongoDbConnection mongoDbConnectionService)
        {
            mongoDbConnectionService.Database.DropCollection("building");
            mongoDbConnectionService.Database.CreateCollection("building");
            _buildingCollection = mongoDbConnectionService.Database.GetCollection<Building>("building");
        }

        public void AddBuilding(Building newBuilding) => _buildingCollection.InsertOne(newBuilding);

        public List<Building> GetAllBuildingsFromBuilder(string builderId)
            => _buildingCollection.Find(building => building.BuilderId == builderId).ToList();

        public Building GetSingleBuildingFromBuilder(string builderId, string buildingId)
            => _buildingCollection.Find(building => building.BuilderId == builderId && building.Id == buildingId).FirstOrDefault();

        public void RemoveBuilding(Building building)
            => _buildingCollection.DeleteOne(GetBuildingFilterById(building.Id));
        public List<Building> GetAllBuildings()
            => _buildingCollection.Find(FilterDefinition<Building>.Empty).ToList();

        public void ReplaceBuildingFromData(Building newBuildingData)
            => _buildingCollection.ReplaceOne(GetBuildingFilterById(newBuildingData.Id), newBuildingData);

        private FilterDefinition<Building> GetBuildingFilterById(string buildingId)
            => Builders<Building>.Filter.Eq("Id", buildingId);
    }
}
