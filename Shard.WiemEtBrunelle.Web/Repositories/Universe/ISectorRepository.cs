using Shard.WiemEtBrunelle.Web.Models.Universe;
using System.Collections.Generic;

namespace Shard.WiemEtBrunelle.Web.Repositories.Universe
{
    public interface ISectorRepository
    {

        void InitializeSector(Sector sector);

        List<StarSystem> GetAllStarSystems();

        StarSystem GetStarSystemByName(string starSystemName);

        Planet GetPlanetFromStarSystemByName(string starSystemName, string planetName);

        void UpdateSystemWithNewData(StarSystem newStarSystemData);
    }
}
