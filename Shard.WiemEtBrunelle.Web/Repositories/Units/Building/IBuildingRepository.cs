using Shard.WiemEtBrunelle.Web.Models.Units;
using System.Collections.Generic;

namespace Shard.WiemEtBrunelle.Web.Repositories
{
    public interface IBuildingRepository
    {
        void AddBuilding(Building newBuilding);

        List<Building> GetAllBuildingsFromBuilder(string builderId);

        Building GetSingleBuildingFromBuilder(string builderId, string buildingId);

        void RemoveBuilding(Building building);

        List<Building> GetAllBuildings();

        void ReplaceBuildingFromData(Building newBuildingData);
    }
}
