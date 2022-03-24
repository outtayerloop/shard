using Shard.Shared.Core;
using Shard.WiemEtBrunelle.Web.Constants.Resources;
using Shard.WiemEtBrunelle.Web.Constants.Units;
using Shard.WiemEtBrunelle.Web.Dto.Units;
using Shard.WiemEtBrunelle.Web.Models.Units;
using Shard.WiemEtBrunelle.Web.Models.Units.GeographicData;
using System;
using System.Collections.Generic;

namespace Shard.WiemEtBrunelle.Web.Converters.Units
{
    public static class UnitConverter
    {
        public static UnitDto ConvertToUnitDto(GenericUnit unitModel)
        {
            if (unitModel == null) 
                return null;

            UnitDto unitDto = GetUnitBasicData(unitModel);
            if(unitModel.UnitType!=UnitConstants.CargoType && unitModel.UnitDestination!=null)
                FillUnitDestinationData(unitDto, unitModel.UnitDestination);

            return unitDto;
        }

        private static UnitDto GetUnitBasicData(GenericUnit unitModel)
        {
            var unitDto = new UnitDto();
            unitDto.Id = unitModel.Id;

            if (unitModel.UnitLocation != null)
            {
                unitDto.System = unitModel.UnitLocation.StarSystem?.Name;
                unitDto.Planet = unitModel.UnitLocation.Planet?.Name;
            }

            unitDto.Type = unitModel.UnitType;

            FillUnitHealthData(unitModel, unitDto);
            FillUnitResourcesQuantity(unitModel, unitDto);

            return unitDto;
        }

        private static void FillUnitHealthData(GenericUnit unitModel, UnitDto unitDto)
        {
            if (IsBattleUnit(unitDto.Type))
                unitDto.Health = unitModel.Health;
            else
                unitDto.Health = null;
        }
        private static void FillUnitResourcesQuantity(GenericUnit unitModel, UnitDto unitDto)
        {
            if (IsCargoUnit(unitDto.Type) && unitModel.ResourcesQuantity!=null)
                FillEachResourcesQuantity(unitModel, unitDto);
            else
                unitDto.ResourcesQuantity = null;
        }
        private static bool IsCargoUnit(string unitType)
        => unitType == UnitConstants.CargoType;

        private static bool IsBattleUnit(string unitType)
            => unitType != UnitConstants.ScoutType || unitType != UnitConstants.BuilderType;

        private static void FillUnitDestinationData(UnitDto unitDto, UnitDestination unitDestination)
        {
            unitDto.DestinationSystem = unitDestination.StarSystem.Name;
            unitDto.DestinationPlanet = unitDestination.Planet?.Name ?? null;
            unitDto.EstimatedTimeOfArrival = DateTime.Parse(unitDestination.EstimatedTimeOfArrival.ToString()).ToString();
        }
        private static void FillEachResourcesQuantity(GenericUnit unitModel, UnitDto unitDto) 
        {
            Dictionary<string, int> defaultResources = new Dictionary<string, int>()
            {
                { "carbon",  0},
                { "iron", 0},
                {  "gold", 0},
                { "aluminium", 0},
                {  "titanium", 0 },
                {  "water", 0},
                {  "oxygen", 0}
            };

            unitDto.ResourcesQuantity = defaultResources;

            foreach (KeyValuePair<ResourceKind,int> resource in unitModel.ResourcesQuantity)
            {
                unitDto.ResourcesQuantity[ResourcesConstants.resourcesName2[resource.Key]] = unitModel.ResourcesQuantity[resource.Key];
            }
        }

    }
}
