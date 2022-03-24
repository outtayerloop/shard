using Shard.WiemEtBrunelle.Web.Dto;
using System.Collections.Generic;

namespace Shard.WiemEtBrunelle.Web.Services
{
    public interface ISectorService
    {
        List<StarSystemDto> GetAllSystems();

        StarSystemDto GetSystemByName(string systemName);

        List<PlanetDto> GetAllPlanetsFromSystem(string systemName);

        PlanetDto GetSinglePlanetFromSystem(string systemName, string planetName);
    }
}