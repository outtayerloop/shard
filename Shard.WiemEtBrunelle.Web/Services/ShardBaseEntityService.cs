using Microsoft.Extensions.Configuration;
using Shard.Shared.Core;
using Shard.WiemEtBrunelle.Web.Converters.Universe;
using Shard.WiemEtBrunelle.Web.Dto;
using Shard.WiemEtBrunelle.Web.Models.Universe;
using Shard.WiemEtBrunelle.Web.Repositories.Universe;
using System.Collections.Generic;
using System.Linq;

namespace Shard.WiemEtBrunelle.Web.Services
{
    public class ShardBaseEntityService
    {

        protected readonly IConfiguration _configuration;
        protected readonly ISectorRepository _sectorRepository;

        public ShardBaseEntityService(IConfiguration configuration, ISectorRepository sectorRepository)
        {
            _configuration = configuration;
            _sectorRepository = sectorRepository;
        }

        protected StarSystem GetSystemModelByName(string systemName)
            => _sectorRepository.GetStarSystemByName(systemName);

        protected StarSystemDto GetSystemDtoByName(string systemName)
        {
            StarSystem systemModel = GetSystemModelByName(systemName);
            return SystemConverter.ConvertToSystemDto(systemModel);
        }

        protected PlanetDto GetSinglePlanetDtoFromSystem(string systemName, string planetName)
        {
            StarSystemDto system = GetSystemDtoByName(systemName);

            if (system == null)
                return null;

            PlanetDto planet = GetPlanetDtoByName(planetName, system.Planets.ToList());

            return planet ?? new PlanetDto(null, null);
        }

        protected Planet GetSinglePlanetModelFromSystem(string systemName, string planetName)
        {
            StarSystem system = GetSystemModelByName(systemName);

            if (system == null) 
                return null;

            Planet planet = GetPlanetModelByName(planetName, system.Planets.ToList());

            return planet ?? new Planet(null, null, null);
        }

        protected Planet GetPlanetModelByName(string planetName, List<Planet> planets)
            => planets.Where(planet => planet.Name == planetName).FirstOrDefault();

        protected PlanetDto GetPlanetDtoByName(string planetName, List<PlanetDto> planets)
            => planets.Where(planet => planet.Name == planetName).FirstOrDefault();
    }
}