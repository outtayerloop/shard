using Shard.WiemEtBrunelle.Web.Dto;
using Shard.WiemEtBrunelle.Web.Models.Universe;
using System.Collections.Generic;

namespace Shard.WiemEtBrunelle.Web.Converters.Universe
{
    public static class SystemConverter
    {
        public static StarSystemDto ConvertToSystemDto(StarSystem systemModel)
        {
            if (systemModel == null) 
                return null;

            IReadOnlyList<PlanetDto> planetDtoList = ConvertToPlanetDto(systemModel.Planets);
            var systemDto = new StarSystemDto(planetDtoList, systemModel.Name);
            return systemDto;
        }

        private static IReadOnlyList<PlanetDto> ConvertToPlanetDto(IReadOnlyList<Planet> planetModelList)
        {
            var planetDtoList = new List<PlanetDto>();
            foreach(Planet planetModel in planetModelList)
            {
                var planetDto = new PlanetDto(planetModel.Name, planetModel.Size);
                planetDtoList.Add(planetDto);
            }
            return planetDtoList;
        }
    }
}
