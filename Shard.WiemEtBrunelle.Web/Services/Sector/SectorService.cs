using Microsoft.Extensions.Configuration;
using Shard.WiemEtBrunelle.Web.Converters.Universe;
using Shard.WiemEtBrunelle.Web.Dto;
using Shard.WiemEtBrunelle.Web.Models.Universe;
using Shard.WiemEtBrunelle.Web.Repositories.Universe;
using System.Collections.Generic;
using System.Linq;

namespace Shard.WiemEtBrunelle.Web.Services
{
    public class SectorService : ShardBaseEntityService, ISectorService
    {
        public SectorService(IConfiguration configuration, ISectorRepository sectorRepository) : base(configuration, sectorRepository) {}

        public List<StarSystemDto> GetAllSystems() 
        {
            var systemDtoList = new List<StarSystemDto>();
            _sectorRepository.GetAllStarSystems().ForEach(systemModel => AddNewConvertedSystemToDtoList(systemDtoList, systemModel));
            return systemDtoList;
        }

        public StarSystemDto GetSystemByName(string systemName)
            => GetSystemDtoByName(systemName);

        public List<PlanetDto> GetAllPlanetsFromSystem(string systemName)
        {
            StarSystemDto system = GetSystemDtoByName(systemName);
            if (system == null) 
                return null;
            return system.Planets.ToList();
        }

        public PlanetDto GetSinglePlanetFromSystem(string systemName, string planetName)
            => GetSinglePlanetDtoFromSystem(systemName, planetName);

        private void AddNewConvertedSystemToDtoList(List<StarSystemDto> systemDtoList, StarSystem systemModel)
        {
            StarSystemDto systemDto = SystemConverter.ConvertToSystemDto(systemModel);
            systemDtoList.Add(systemDto);
        }
    }
}
