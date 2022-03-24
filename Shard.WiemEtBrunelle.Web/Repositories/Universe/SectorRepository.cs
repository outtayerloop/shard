using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Shard.Shared.Core;
using Shard.WiemEtBrunelle.Web.Converters.Universe;
using Shard.WiemEtBrunelle.Web.Database.Services;
using Shard.WiemEtBrunelle.Web.Models.Universe;
using System.Collections.Generic;
using System.Linq;

namespace Shard.WiemEtBrunelle.Web.Repositories.Universe
{
    public class SectorRepository : ISectorRepository
    {
        
        private readonly IMongoCollection<StarSystem> _starSystemCollection;
        private readonly IConfiguration _configuration;

        public SectorRepository(IConfiguration configuration, MongoDbConnection mongoDbConnectionService)
        {
            _configuration = configuration;
            mongoDbConnectionService.Database.DropCollection("starsystem");
            mongoDbConnectionService.Database.CreateCollection("starsystem");
            _starSystemCollection = mongoDbConnectionService.Database.GetCollection<StarSystem>("starsystem");
            InitializeSector(GetSector());
        }

        public void InitializeSector(Sector sector)
            => sector.Systems.ToList().ForEach(starSystem => AddNewStarSystemToCollection(starSystem));

        public List<StarSystem> GetAllStarSystems()
            => _starSystemCollection.Find(FilterDefinition<StarSystem>.Empty).ToList();

        public StarSystem GetStarSystemByName(string starSystemName)
           => _starSystemCollection.Find(starSystem => starSystem.Name == starSystemName).FirstOrDefault();

        public Planet GetPlanetFromStarSystemByName(string starSystemName, string planetName)
        {
            StarSystem system = GetStarSystemByName(starSystemName);

            if (system == null)
                return null;

            Planet planet = GetPlanetModelByName(planetName, system.Planets);

            return planet ?? new Planet(null, null, null);
        }

        public void UpdateSystemWithNewData(StarSystem newStarSystemData)
            => _starSystemCollection.ReplaceOne(GetUserFilterByName(newStarSystemData.Name), newStarSystemData);

        private void AddNewStarSystemToCollection(StarSystem starSystemToAdd)
            => _starSystemCollection.InsertOne(starSystemToAdd);

        private Planet GetPlanetModelByName(string planetName, List<Planet> planets)
            => planets.Where(planet => planet.Name == planetName).FirstOrDefault();

        private FilterDefinition<StarSystem> GetUserFilterByName(string starSystemName)
            => Builders<StarSystem>.Filter.Eq("Name", starSystemName);

        private Sector GetSector()
        {
            var options = new MapGeneratorOptions { Seed = _configuration.GetValue<string>("Seed") };
            var mapGenerator = new MapGenerator(options);
            SectorSpecification sectorSpecification = mapGenerator.Generate();
            return SectorSpecificationConverter.ConvertToSector(sectorSpecification);
        }
    }
}
