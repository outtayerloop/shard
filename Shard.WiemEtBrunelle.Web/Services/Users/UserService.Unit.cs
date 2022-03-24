using Shard.Shared.Core;
using Shard.WiemEtBrunelle.Web.Constants;
using Shard.WiemEtBrunelle.Web.Constants.Requests;
using Shard.WiemEtBrunelle.Web.Constants.Resources;
using Shard.WiemEtBrunelle.Web.Constants.Units;
using Shard.WiemEtBrunelle.Web.Converters.Units;
using Shard.WiemEtBrunelle.Web.Dto.Units;
using Shard.WiemEtBrunelle.Web.Models.Units;
using Shard.WiemEtBrunelle.Web.Models.Units.Battle;
using Shard.WiemEtBrunelle.Web.Models.Units.GeographicData;
using Shard.WiemEtBrunelle.Web.Models.Universe;
using Shard.WiemEtBrunelle.Web.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shard.WiemEtBrunelle.Web.Services.Users
{
    public partial class UserService : ShardBaseEntityService, IUserService
    {
        public async Task<UnitDto> GetUnitOfSpecificType(string userId, string unitType)
        {
            User userFound = _userRepository.GetUser(userId);

            if (userFound == null) 
                return null;

            GenericUnit unitModel = userFound.Units.Where(unit => unit.UnitType == unitType).FirstOrDefault();

            if (unitModel == null) 
                return new UnitDto(null, null, null, null);

            if (unitModel.UnitDestination.Planet == null)
            {
                GivePlanetDestinationToNewBuilder(unitModel);
            }
            if (unitModel.UnitLocation.Planet == null)
            {
                GivePlanetLocationToNewBuilder(unitModel);
            }

            return await GetSingleUnitFromUser(userId, unitModel.Id);
        }

        public UnitDto UpdateSingleUnitFromUser(string userId, UnitDto newUnitData)
        {
            GenericUnit unitModelToUpdate = GetUnitModelFromUser(userId, newUnitData.Id);

            if (unitModelToUpdate == null || UnitIsNotCargoButHasResources(newUnitData, unitModelToUpdate)) 
                return null;

            if (unitModelToUpdate.Id == null ) 
                return new UnitDto(null, null, null, null);

            if (DataToUpdateIsResourcesQuantity(newUnitData) && UnitToUpdateIsCargoUnit(unitModelToUpdate) && !DataToUpdateIsDestinationSystem(newUnitData))
                return UpdateResourcesForCargoUnit(unitModelToUpdate, newUnitData, userId);

            StarSystem newSystemDestinationModel = GetSystemModelByName(newUnitData.DestinationSystem);

            if (newSystemDestinationModel == null && !UnitToUpdateIsCargoUnit(unitModelToUpdate))
                return new UnitDto(unitModelToUpdate.Id, null, null, null);

            UnitDto updatedUnitDto =  GetUpdatedUnitDto(newUnitData, unitModelToUpdate, newSystemDestinationModel);

            UpdateSingleUnitForDatabaseUser(userId, unitModelToUpdate);

            return updatedUnitDto;
        }

        private UnitDto UpdateResourcesForCargoUnit(GenericUnit unitModelToUpdate, UnitDto newUnitData, string userId)
        {
            unitModelToUpdate = LoadOrUnloadResourcesInCargo(newUnitData, unitModelToUpdate, userId);

            UpdateSingleUnitForDatabaseUser(userId, unitModelToUpdate);

            return unitModelToUpdate == null
                ? null
                : UnitConverter.ConvertToUnitDto(unitModelToUpdate);
        }

        public UnitDto PutUnitAsAdministrator(string userId, UnitDto unit)
        {
            GenericUnit newUnit = GetGenericUnitModelCreatedByAdmin(unit);

            User foundUser = _userRepository.GetUser(userId);

            if(foundUser != null)
                foundUser.Units.Add(newUnit);

            ChangeAdministratorStatusToUnauthenticated();

            _userRepository.ReplaceUserWithNewData(foundUser);

            return new UnitDto() { 
                Id = unit.Id, 
                Type = unit.Type, 
                System = unit.System, 
                Planet = unit.Planet, 
                DestinationPlanet = unit.Planet, 
                DestinationSystem = unit.System
            };

        }

        public UnitDto ReceivingJumpingCargo(string userId, UnitDto jumpingUnitData)
        {
            User userFound = _userRepository.GetUser(userId);
            GenericUnit jumpingCargo = new CargoUnit(jumpingUnitData.Id);
            jumpingCargo.Health = (int)jumpingUnitData.Health;

            ResourcelessUnitLocation location = new ResourcelessUnitLocation();
            jumpingCargo.UnitLocation = location;
            StarSystem system = new StarSystem(new List<Planet>(), "80ad7191-ef3c-14f0-7be8-e875dad4cfa6");
            jumpingCargo.UnitLocation.StarSystem = system;

            ConvertStringResourcesToResourceKindResources(jumpingCargo, jumpingUnitData);

            userFound.Units.Add(jumpingCargo);

            _userRepository.ReplaceUserWithNewData(userFound);

            return UnitConverter.ConvertToUnitDto(jumpingCargo);
            
        }

        private void ConvertStringResourcesToResourceKindResources(GenericUnit jumpingCargo, UnitDto jumpingUnitData)
        {
            jumpingCargo.ResourcesQuantity = new Dictionary<ResourceKind, int>() { };
            foreach(KeyValuePair<string, int> resource in jumpingUnitData.ResourcesQuantity)
            {
                jumpingCargo.ResourcesQuantity[ResourcesConstants.resourcesName[resource.Key]] = jumpingUnitData.ResourcesQuantity[resource.Key];
            }

        }

        private GenericUnit GetGenericUnitModelCreatedByAdmin(UnitDto unit)
        {
            GenericUnit newUnit = GetUnitModelInstanceConditionally(unit);

            FillModelLocationFromDtoData(unit, newUnit);

            FillModelDestinationFromDtoData(unit, newUnit);

            return newUnit;
        }

        private void FillModelLocationFromDtoData(UnitDto unit, GenericUnit newUnit)
        {
            newUnit.UnitLocation = GetLocationInstanceConditionally(newUnit.CanScanPlanetResources);
            newUnit.UnitLocation.StarSystem = GetSystemModelByName(unit.System);

            newUnit.UnitLocation.Planet = unit.Planet != null
                ? GetPlanetModelByName(unit.Planet, newUnit.UnitLocation.StarSystem.Planets.ToList())
                : null;
        }

        private void FillModelDestinationFromDtoData(UnitDto unit, GenericUnit newUnit)
        {
            newUnit.UnitDestination = new UnitDestination()
            {
                StarSystem = GetSystemModelByName(unit.System),
                Planet = GetPlanetDestinationFromDtoData(unit, newUnit),
                EstimatedTimeOfArrival = TimeSpan.FromSeconds(0),
                DateOfArrival = _systemClockService.Now,
                DateOfEntryInNewSystem = _systemClockService.Now
            };
        }

        private Planet GetPlanetDestinationFromDtoData(UnitDto unit, GenericUnit newUnit)
        {
            return unit.Planet == null
                ? null
                : GetPlanetModelByName(unit.Planet, newUnit.UnitLocation.StarSystem.Planets.ToList());
        }

        private void ChangeAdministratorStatusToUnauthenticated()
            => RequestConstants.AdminIsAuthenticated = false;

        /// <summary>
        /// Retourne null si l'utilisateur est introuvable, une liste contenant un élément null si l'utilisateur
        /// n'a aucun vaisseau, sinon une liste contenant les vaisseaux de l'utilisateur
        /// </summary>
        /// <param name="userId">Id de l'utilisateur dont on veut récupérer les vaisseaux</param>
        /// <returns></returns>
        private List<GenericUnit> GetUnitModelListFromUser(string userId)
        {
            User foundUser = _userRepository.GetUser(userId);

            if (foundUser == null) 
                return null;

            return foundUser.Units;
        }

        /// <summary>
        /// Ajoute un nouveau DTO de vaisseau à la liste associée à partir d'un modèle de vaisseau
        /// </summary>
        /// <param name="unitModel">Modèle de vaisseau utilisé</param>
        /// <param name="unitDtoList">Liste de DTOs de vaisseaux remplie</param>
        private void AddNewConvertedUnitToDtoList(GenericUnit unitModel, List<UnitDto> unitDtoList)
            => unitDtoList.Add(UnitConverter.ConvertToUnitDto(unitModel));

        /// <summary>
        /// Détermine si le vaisseau recherché dans la liste de modèles n'existe pas
        /// </summary>
        /// <param name="unitId">ID de vaisseau recherché</param>
        /// <param name="unitModelList">Liste de modèles de vaisseaux analysée</param>
        /// <returns></returns>
        private bool IsUnitNotFound(string unitId, List<GenericUnit> unitModelList)
            => DoesNotContainAnyUnitModel(unitModelList) || DoesNotContainSearchedUnit(unitId, unitModelList);

        private bool DoesNotContainSearchedUnit(string unitId, List<GenericUnit> unitModelList)
            => GetUnitDtoFromModelList(unitId, unitModelList) == null;

        /// <summary>
        /// Détermine si une liste de modèles de vaisseaux ne contient que des modèles null
        /// </summary>
        /// <param name="unitModelList">Liste de modèles de vaisseaux parcourue</param>
        /// <returns></returns>
        private bool DoesNotContainAnyUnitModel(List<GenericUnit> unitModelList)
            => unitModelList.All(unitModel => unitModel == null);

        /// <summary>
        /// Retourne null si l'ID de vaisseau est introuvable dans la liste de modèles de vaisseaux,
        /// sinon retourne un DTO de vaisseau obtenu après conversion du modèle de vaisseau obtenu
        /// </summary>
        /// <param name="unitId">ID de vaisseau recherché</param>
        /// <param name="unitModelList">Liste de modèles de vaisseaux parcourue</param>
        /// <returns></returns>
        private UnitDto GetUnitDtoFromModelList(string unitId, List<GenericUnit> unitModelList)
        {
            GenericUnit unitModel = GetUnitById(unitId, unitModelList);
            return UnitConverter.ConvertToUnitDto(unitModel);
        }

        private GenericUnit GetUnitModelFromUser(string userId, string unitId)
        {
            List<GenericUnit> unitModelList = GetUnitModelListFromUser(userId);

            if (unitModelList == null) 
                return null;

            if (IsUnitNotFound(unitId, unitModelList)) 
                return new GenericUnit(null);

            return GetUnitById(unitId, unitModelList);
        }

        private GenericUnit GetUnitById(string unitId, List<GenericUnit> units)
            => units.Where(unit => unit.Id == unitId).FirstOrDefault();

        private UnitDto GetUpdatedUnitDto(UnitDto newUnitDataDto, GenericUnit unitModelToUpdate,
            StarSystem newDestinationSystemModel)
        {

            Planet newDestinationPlanetModel = null;

            if (newUnitDataDto.DestinationPlanet != null)
            {
                return GetUpdatedUnitModelWithNewPlanet(newUnitDataDto, unitModelToUpdate, newDestinationSystemModel, out newDestinationPlanetModel);
            }

            return GetUnitDtoFromUpdatedUnitModel(unitModelToUpdate, newDestinationSystemModel, newDestinationPlanetModel);
        }

        private UnitDto GetUpdatedUnitModelWithNewPlanet(UnitDto newUnitDataDto, GenericUnit unitModelToUpdate,
            StarSystem newDestinationSystemModel, out Planet newDestinationPlanetModel)
        {
            newDestinationPlanetModel = GetSinglePlanetModelFromSystem(newDestinationSystemModel.Name, newUnitDataDto.DestinationPlanet);

            if (newDestinationPlanetModel.Name == null) return new UnitDto(unitModelToUpdate.Id,
                unitModelToUpdate.UnitLocation.StarSystem.Name, EntityNotFoundConstants.EntityNotFoundMessage, null);

            return GetUnitDtoFromUpdatedUnitModel(unitModelToUpdate, newDestinationSystemModel, newDestinationPlanetModel);
        }

        private UnitDto GetUnitDtoFromUpdatedUnitModel(GenericUnit unitModelToUpdate,
            StarSystem newDestinationSystemModel, Planet newDestinationPlanetModel)
        {
            GenericUnit updatedUnitModel = GetUpdatedUnitModelAtMoveOrder(newDestinationPlanetModel, unitModelToUpdate, newDestinationSystemModel);
            return UnitConverter.ConvertToUnitDto(updatedUnitModel);
        }

        private GenericUnit GetUpdatedUnitModelAtMoveOrder(Planet newDestinationPlanetModel,
            GenericUnit unitModelToUpdate, StarSystem newDestinationSystemModel)
        {

            unitModelToUpdate.UnitDestination = GetUpdatedUnitDestination(unitModelToUpdate, newDestinationSystemModel,
                newDestinationPlanetModel);

            UpdateUnitLocationAtMoveOrder(unitModelToUpdate, newDestinationSystemModel.Name, newDestinationPlanetModel?.Name);

            return unitModelToUpdate;
        }

        private void AssociateInitialUnitLocation(List<GenericUnit> unitList)
        {
            StarSystem system = GetRandomEntityFromList(_sectorRepository.GetAllStarSystems());
            unitList.ForEach(unit => unit.UnitLocation = GetInitialUnitLocation(unit.CanScanPlanetResources, system));
        }

        private void GivePlanetDestinationToNewBuilder(GenericUnit builder)
        {
            string systemName = builder.UnitLocation.StarSystem.Name;
            StarSystem system = GetSystemModelByName(systemName);
            Planet planetDestination = GetRandomEntityFromList(system.Planets.ToList());
            builder.UnitDestination.Planet = planetDestination;
        }

        private void GivePlanetLocationToNewBuilder(GenericUnit builder)
        {
            string systemName = builder.UnitLocation.StarSystem.Name;
            StarSystem system = GetSystemModelByName(systemName);
            Planet planetLocation = GetRandomEntityFromList(system.Planets.ToList());
            builder.UnitLocation.Planet = planetLocation;
        }

        private GenericUnit GetUnitModelInstanceConditionally(UnitDto unit)
        {
            switch (unit.Type)
            {
                case "scout": return new ScoutUnit(unit.Id);
                case "builder": return new ScoutUnit(unit.Id);
                case "fighter": return new FighterUnit(unit.Id);
                case "cruiser": return new CruiserUnit(unit.Id);
                case "bomber": return new BomberUnit(unit.Id);
                case "cargo": return new CargoUnit(unit.Id);
                default: throw new NotImplementedException(); 
            }
        }

        private bool UnitIsNotCargoButHasResources(UnitDto newUnitData, GenericUnit unitModelToUpdate)
            => newUnitData.ResourcesQuantity != null && unitModelToUpdate.UnitType != UnitConstants.CargoType;

        private bool UnitToUpdateIsCargoUnit(GenericUnit unitModelToUpdate)
            => unitModelToUpdate.UnitType == UnitConstants.CargoType;

        private bool DataToUpdateIsDestinationSystem(UnitDto newUnitData)
            => newUnitData.DestinationSystem != null;

        private bool DataToUpdateIsResourcesQuantity(UnitDto newUnitData)
            => newUnitData.ResourcesQuantity != null;
    }
}
