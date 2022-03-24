using Shard.Shared.Core;
using Shard.WiemEtBrunelle.Web.Constants.Units;
using Shard.WiemEtBrunelle.Web.Converters.Units;
using Shard.WiemEtBrunelle.Web.Dto.Units;
using Shard.WiemEtBrunelle.Web.Models.Units;
using Shard.WiemEtBrunelle.Web.Models.Units.GeographicData;
using Shard.WiemEtBrunelle.Web.Models.Universe;
using Shard.WiemEtBrunelle.Web.Models.Users;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shard.WiemEtBrunelle.Web.Services.Users
{
    public partial class UserService : ShardBaseEntityService, IUserService
    {
        public async Task<UnitDto> StartPortCreatesUnitAsync(string userId, string startportId, UnitDto newUnitData)
        {

            BuildingDto starport = await GetSingleBuildingFromUser(userId, startportId); 
            User user = _userRepository.GetUser(userId);

            if (IsBuildingBuilt(starport) && UserHasEnoughResources(user))
            {
                UserPayForUnitConstruction(user, newUnitData);

                GenericUnit unitModelCreatedByStarport = StarportBuildUnit(user, newUnitData, starport);

                HandlerEntitiesPersistance(starport, unitModelCreatedByStarport);
             
                return UnitConverter.ConvertToUnitDto(unitModelCreatedByStarport);

            }

            ChangeAdministratorStatusToUnauthenticated();
            return null;
        }

        private void HandlerEntitiesPersistance(BuildingDto starportDto, GenericUnit unitModelCreatedByStarport)
        {
            Building starportModel = AddNewUnitToBuildingModelUnitQueue(starportDto, unitModelCreatedByStarport);

            AddStarportToPlanetStarportList(starportModel);
           
            _buildingRepository.AddBuilding(starportModel);
        }

        private Building AddNewUnitToBuildingModelUnitQueue(BuildingDto starportDto, GenericUnit unitModelCreatedByStarport)
        {
            Building starportModel = new Building();
            starportModel.Id = starportDto.Id;
            starportModel.Type = starportDto.Type;
            starportModel.BuilderId = starportDto.BuilderId;
            starportModel.IsBuilt = starportDto.IsBuilt;
            starportModel.UnitsQueue = new List<GenericUnit>() { unitModelCreatedByStarport };
            starportModel.StarSystem = starportDto.System;
            starportModel.Planet = starportDto.Planet;

            return starportModel;
        }

        private void AddStarportToPlanetStarportList(Building starportModel)
        {
            Planet planet = GetSinglePlanetModelFromSystem(starportModel.StarSystem, starportModel.Planet);
            planet.Starports = new List<Building>();
            planet.Starports.Add(starportModel);
            StarSystem starSystemToUpdate = _sectorRepository.GetStarSystemByName(starportModel.StarSystem);
            Planet planetToUpdate = starSystemToUpdate.Planets.Find(planet => planet.Name == planet.Name);
            planetToUpdate = planet;
            _sectorRepository.UpdateSystemWithNewData(starSystemToUpdate);
        }


        private GenericUnit StarportBuildUnit(User user, UnitDto newUnitData, BuildingDto startportDto)
        {
            GenericUnit newUnit = new GenericUnit() ;

            if (IsBuilderUnit(newUnitData))
                newUnit = new BuilderUnit(Guid.NewGuid().ToString());
            
            if (IsScoutUnit(newUnitData))
                newUnit = new ScoutUnit(Guid.NewGuid().ToString());

            if (IsCargoUnit(newUnitData))
                newUnit = new CargoUnit(Guid.NewGuid().ToString());

            FillNewUnitProperties(user, newUnit, startportDto);
           
            return newUnit;
        }

        private void FillNewUnitProperties(User user, GenericUnit newUnit, BuildingDto startportDto)
        {
            BaseUnitLocation unitLocation = new ResourcelessUnitLocation();
            unitLocation.StarSystem = GetSystemModelByName(startportDto.System);
            unitLocation.Planet = GetSinglePlanetModelFromSystem(startportDto.System, startportDto.Planet);
            newUnit.UnitLocation = unitLocation;
            user.Units.Add(newUnit);
            _userRepository.ReplaceUserWithNewData(user);
        }

        private bool UserHasEnoughResources(User user)
        {
            return user.ResourcesQuantity[ResourceKind.Carbon] >= BuildingConstants.CostScoutConstruction[ResourceKind.Carbon]
                && user.ResourcesQuantity[ResourceKind.Iron] >= BuildingConstants.CostScoutConstruction[ResourceKind.Iron];
        }

        private void UserPayForUnitConstruction(User user, UnitDto newUnitData)
        {
            if (IsScoutUnit(newUnitData))
                BuildScoutCostResources(user);
            
            if (IsBuilderUnit(newUnitData))
                BuildBuilderCostResources(user);

            if (IsCargoUnit(newUnitData))
                BuildCargoCostResources(user);

        }

        private void BuildScoutCostResources(User user)
        {
            user.ResourcesQuantity[ResourceKind.Carbon] -= BuildingConstants.CostScoutConstruction[ResourceKind.Carbon];
            user.ResourcesQuantity[ResourceKind.Iron] -= BuildingConstants.CostScoutConstruction[ResourceKind.Iron];
        }

        private void BuildBuilderCostResources(User user)
        {
            user.ResourcesQuantity[ResourceKind.Carbon] -= BuildingConstants.CostBuilderConstruction[ResourceKind.Carbon];
            user.ResourcesQuantity[ResourceKind.Iron] -= BuildingConstants.CostBuilderConstruction[ResourceKind.Iron];
        }
        private void BuildCargoCostResources(User user)
        {
            user.ResourcesQuantity[ResourceKind.Carbon] -= BuildingConstants.CostCargoConstruction[ResourceKind.Carbon];
            user.ResourcesQuantity[ResourceKind.Iron] -= BuildingConstants.CostCargoConstruction[ResourceKind.Iron];
            user.ResourcesQuantity[ResourceKind.Gold] -= BuildingConstants.CostCargoConstruction[ResourceKind.Gold];
        }

        private bool IsBuildingBuilt(BuildingDto building)
        => building.IsBuilt;

        private bool IsCargoUnit(UnitDto newUnitData)
        => newUnitData.Type == UnitConstants.CargoType;

        private bool IsBuilderUnit(UnitDto newUnitData)
        => newUnitData.Type == UnitConstants.BuilderType;

        private bool IsScoutUnit(UnitDto newUnitData)
        => newUnitData.Type == UnitConstants.ScoutType;
    }
}
