using Shard.WiemEtBrunelle.Web.Dto.Units;
using Shard.WiemEtBrunelle.Web.Models.Units;

namespace Shard.WiemEtBrunelle.Web.Converters
{
    public static class BuildingConverter
    {
        public static BuildingDto ConvertToBuildingDto(Building buildingModel)
        {
            if (buildingModel == null) 
                return null;

            var buildingDto = new BuildingDto()
            {
                Id = buildingModel.Id,
                Type = buildingModel.Type,
                ResourceCategory = buildingModel.ResourceCategory,
                BuilderId = buildingModel.BuilderId,
                System = buildingModel.StarSystem,
                Planet = buildingModel.Planet,
                IsBuilt = buildingModel.IsBuilt,
                EstimatedBuildTime = buildingModel.EstimatedBuildTime?.ToString() ?? null,
               // UnitsQueue = buildingModel.UnitsQueue
            };

            return buildingDto;
        }
    }
}
