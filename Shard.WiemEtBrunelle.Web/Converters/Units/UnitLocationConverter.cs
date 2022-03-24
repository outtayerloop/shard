using Shard.Shared.Core;
using Shard.WiemEtBrunelle.Web.Dto.Units;
using Shard.WiemEtBrunelle.Web.Models.Units.GeographicData;
using System.Collections.Generic;

namespace Shard.WiemEtBrunelle.Web.Converters.Units
{
    public static class UnitLocationConverter
    {
        public static GenericUnitLocationDto ConvertToUnitLocationDto(BaseUnitLocation unitLocationModel)
        {
            if (unitLocationModel == null) 
                return null;

            Dictionary<string, int> resourceQuantity = GetConvertedResourceQuantity(unitLocationModel);
            GenericUnitLocationDto unitLocationDto = CreateUnitLocationDtoConditionally(unitLocationModel, resourceQuantity);
            return unitLocationDto;
        }

        private static Dictionary<string, int> GetConvertedResourceQuantity(BaseUnitLocation unitLocationModel)
        {
            IReadOnlyDictionary<ResourceKind, int> modelResources = unitLocationModel.Planet?.ResourceQuantity ?? null;
            Dictionary<string, int> resourceQuantity = ResourcesConverter.GetLowerCasedResources(modelResources);
            return resourceQuantity;
        }

        private static GenericUnitLocationDto CreateUnitLocationDtoConditionally(BaseUnitLocation unitLocationModel, Dictionary<string, int> resourceQuantity)
        {
            return unitLocationModel.HasResources() 
                ? CreateResourcefulUnitLocationDto(unitLocationModel, resourceQuantity) 
                : CreateResourcelessUnitLocationDto(unitLocationModel);
        }

        private static UnitLocationWithResourcesDto CreateResourcefulUnitLocationDto(BaseUnitLocation unitLocationModel, Dictionary<string, int> resourceQuantity)
        {
            return new UnitLocationWithResourcesDto(resourceQuantity)
            {
                System = unitLocationModel.StarSystem.Name,
                Planet = GetUnitLocationDtoPlanet(unitLocationModel)
            };
        }

        private static GenericUnitLocationDto CreateResourcelessUnitLocationDto(BaseUnitLocation unitLocationModel)
        {
            return new GenericUnitLocationDto()
            {
                System = unitLocationModel.StarSystem.Name,
                Planet = GetUnitLocationDtoPlanet(unitLocationModel)
            };
        }

        private static string GetUnitLocationDtoPlanet(BaseUnitLocation unitLocationModel)
            => unitLocationModel.Planet?.Name;
    }
}
