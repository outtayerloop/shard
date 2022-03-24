using Microsoft.Extensions.Configuration;
using Shard.Shared.Core;
using Shard.WiemEtBrunelle.Web.Constants.Requests;
using Shard.WiemEtBrunelle.Web.Constants.Resources;
using Shard.WiemEtBrunelle.Web.Constants.Units;
using Shard.WiemEtBrunelle.Web.Converters.Units;
using Shard.WiemEtBrunelle.Web.Converters.Users;
using Shard.WiemEtBrunelle.Web.Dto.Units;
using Shard.WiemEtBrunelle.Web.Dto.Users;
using Shard.WiemEtBrunelle.Web.Models.Units;
using Shard.WiemEtBrunelle.Web.Models.Users;
using Shard.WiemEtBrunelle.Web.Repositories;
using Shard.WiemEtBrunelle.Web.Repositories.Universe;
using Shard.WiemEtBrunelle.Web.Repositories.Users;
using Shard.WiemEtBrunelle.Web.Utils.Units;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Shard.WiemEtBrunelle.Web.Services.Users
{
    public partial class UserService : ShardBaseEntityService, IUserService
    {

        private readonly IClock _systemClockService;
        private readonly IUserRepository _userRepository;
        private readonly IBuildingRepository _buildingRepository;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly ConcurrentBag<Task> _tasksBag;
        private readonly IHttpClientFactory _clientFactory;

        public UserService(IConfiguration configuration, ISectorRepository sectorRepository, IClock systemClockService, 
            IUserRepository userRepository, IBuildingRepository buildingRepository, IHttpClientFactory clientFactory)
            : base(configuration, sectorRepository)
        {
            _systemClockService = systemClockService;
            _userRepository = userRepository;
            _buildingRepository = buildingRepository;
            _cancellationTokenSource = new CancellationTokenSource();
            _tasksBag = new ConcurrentBag<Task>();
            _clientFactory = clientFactory;
        }

        public UserService(IConfiguration configuration, ISectorRepository sectorRepository, IUserRepository userRepository, 
            IBuildingRepository buildingRepository, IHttpClientFactory clientFactory)
            : base(configuration, sectorRepository)
        {
            _userRepository = userRepository;
            _buildingRepository = buildingRepository;
            _clientFactory = clientFactory;
        }

        public UserService(IConfiguration configuration, ISectorRepository sectorRepository, IClock systemClockService, IUserRepository userRepository,
            IBuildingRepository buildingRepository)
            : base(configuration, sectorRepository)
        {
            _userRepository = userRepository;
            _systemClockService = systemClockService;
            _buildingRepository = buildingRepository;
        }

        /// <summary>
        /// Recherche un utilisateur sur la base de son identifiant
        /// </summary>
        /// <param name="userId">L'identifiant d'utilisateur sur lequel se base la recherche</param>
        public UserDto GetUserById(string userId)
        {
            User userFound = _userRepository.GetUser(userId);

            if (userFound == null)
                return null;

            if(userFound.Pseudo != RequestConstants.UserRemotePseudo)
                GiveExtractedResourcesToUser(userId);

            return UserConverter.ConvertToUserDto(userFound);
        }

        private void GiveExtractedResourcesToUser(string userId)
        {
            GenericUnit builderUnit = GetUserBuildingBuilder(userId);

            ExtractResourcesFromUserMineList(userId, builderUnit);
        }

        private void ExtractResourcesFromUserMineList(string userId, GenericUnit builderUnit)
        {
            List<Building> userBuildings = _buildingRepository.GetAllBuildingsFromBuilder(builderUnit.Id);

            for (int i = 0; i < userBuildings.Count; i++)
            {
                UpdateResourcesQuantityForUser(userId, userBuildings[i]);
            }
        }

        /// <summary>
        /// Crée l'utilisateur en lui affectant un nouveau vaisseau scout s'il n'existait pas, sinon modifie son pseudo
        /// et retourne l'utilisateur dans les deux cas
        /// </summary>
        /// <param name="userData">Données de création ou de modificaton d'un utilisateur</param>
        /// <returns></returns>
        public UserDto CreateOrUpdateUser(UserDto userData)
        {
            User foundUserModel = _userRepository.GetUser(userData.Id);

            return foundUserModel == null 
                ? CreateAndReturnUser(userData) 
                : UpdateAndReturnUser(userData, foundUserModel);
        }

        public async Task<List<UnitDto>> GetAllUnitsFromUser(string userId)
        {
            DateTime requestArrivalDate = _systemClockService.Now;
         
            List<GenericUnit> unitModelList = GetUnitModelListFromUser(userId);

            if (unitModelList == null)
                return null;

            return await GetUpdatedUnitDtoList(userId, requestArrivalDate, unitModelList);
        }

        private async Task<List<UnitDto>> GetUpdatedUnitDtoList(string userId, DateTime requestArrivalDate, List<GenericUnit> unitModelList)
        {

            if (unitModelList.Count < 3)
            {
                List<UnitDto> unitDtoList = new List<UnitDto>() { };
                foreach(GenericUnit unit in unitModelList)
                {
                    UnitDto unitDto = UnitConverter.ConvertToUnitDto(unit);
                    unitDtoList.Add(unitDto);
                }
                return unitDtoList;
            }

            for (int i = 0; i < unitModelList.Count; ++i)
            {
                
                unitModelList[i] = await UpdateUnitSpaceTimeData(unitModelList[i], 
                    TimeTiedEntityUpdateContext.Continue, requestArrivalDate);
            }

            UpdateDatabaseUser(userId, unitModelList);

            return GetUnitDtoListFromUnitModelList(unitModelList);
        }

        private List<UnitDto> GetUnitDtoListFromUnitModelList(List<GenericUnit> unitModelList)
        {
            var unitDtoList = new List<UnitDto>();

            unitModelList.ForEach(unitModel => AddNewConvertedUnitToDtoList(unitModel, unitDtoList));
            return unitDtoList;
        }

        public async Task<UnitDto> GetSingleUnitFromUser(string userId, string unitId)
        {
            DateTime requestArrivalDate = _systemClockService.Now;

            List<GenericUnit> unitModelList = GetUnitModelListFromUser(userId);

            if (unitModelList == null)
                return null;

            if (IsUnitNotFound(unitId, unitModelList))
                return new UnitDto(null, null, null, null);


            UnitDto updatedUnitDto =  await GetUpdatedSingleUnitDto(userId, unitId, requestArrivalDate, unitModelList);

            UpdateDatabaseUser(userId, unitModelList);

            return updatedUnitDto;

        }

        private async Task<UnitDto> GetUpdatedSingleUnitDto(string userId, string unitId, 
            DateTime requestArrivalDate, List<GenericUnit> unitModelList)
        {
            GenericUnit foundUnit = GetUnitById(unitId, unitModelList);
            if (foundUnit.UnitType == UnitConstants.CargoType)
                return UnitConverter.ConvertToUnitDto(foundUnit);

            foundUnit = await UpdateUnitSpaceTimeData(foundUnit, 
                TimeTiedEntityUpdateContext.WaitForEntityActionCompletion, requestArrivalDate);

            return GetUnitFinalState(userId, unitModelList, foundUnit);
        }

        private UnitDto GetUnitFinalState(string userId, List<GenericUnit> unitModelList, GenericUnit unit)
        {
            if (IsBattleUnit(unit))
            {
                return ManageBattleAndReturnUnitAfterCombat(userId, unitModelList, unit);
            }

            return UnitConverter.ConvertToUnitDto(unit);
        }

        private UnitDto ManageBattleAndReturnUnitAfterCombat(string userId, List<GenericUnit> unitModelList, GenericUnit battleUnit)
        {
            battleUnit = HandleBattlesBetweenEnemyUnits(userId, battleUnit);
            return GetUnitStateAfterBattle(unitModelList, battleUnit);
        }

        private UnitDto GetUnitStateAfterBattle(List<GenericUnit> unitModelList, GenericUnit battleUnit)
        {
            if (battleUnit.Health == 0)
            {
                return UnitNotFoundAfterBeingDestroyed(unitModelList, battleUnit);
            }
            return UnitConverter.ConvertToUnitDto(battleUnit);
        }

        private UnitDto UnitNotFoundAfterBeingDestroyed(List<GenericUnit> unitModelList, GenericUnit battleUnit)
        {
            unitModelList.Remove(battleUnit);
            return new UnitDto(null, null, null, null);
        }

        public async Task<GenericUnitLocationDto> GetSingleUnitDetailsForUser(string userId, string unitId)
        {
            DateTime requestArrivalDate = _systemClockService.Now;

            GenericUnit unitFound = GetUnitModelFromUser(userId, unitId);

            if (unitFound == null) 
                return null;

            unitFound = await UpdateUnitSpaceTimeData(unitFound, 
                TimeTiedEntityUpdateContext.WaitForEntityActionCompletion, requestArrivalDate);

            UpdateSingleUnitForDatabaseUser(userId, unitFound);

            return UnitLocationConverter.ConvertToUnitLocationDto(unitFound.UnitLocation);
        }

        private void GiveUnitsToNewlyCreatedUser(User newUser)
        {
            CreateUnitListForUser(newUser);
            AssociateInitialUnitLocation(newUser.Units);
            AssociateInitialUnitDestination(newUser.Units);
        }

        private void CreateUnitListForUser(User newUser)
        {
            var scoutUnit = new ScoutUnit(Guid.NewGuid().ToString());
            var builderUnit = new BuilderUnit(Guid.NewGuid().ToString());
            newUser.Units = new List<GenericUnit>() { scoutUnit, builderUnit};
        }

        private UserDto CreateAndReturnUser(UserDto userData)
        {
            User newUser = CreateUserModelFromData(userData);
            _userRepository.AddUser(newUser);

            if (RequestConstants.NewUserIsAuthenticated && newUser.Pseudo == RequestConstants.UserRemotePseudo)
                RemoveResourcesAndUnitsForUserReceived(newUser);

            ChangeUserReceivedStatusToUnauthenticated();
            return UserConverter.ConvertToUserDto(newUser);
        }

        private void RemoveResourcesAndUnitsForUserReceived(User userReceived)
        {
            RemoveResources(userReceived.ResourcesQuantity);
            userReceived.Units = new List<GenericUnit>();
            _userRepository.ReplaceUserWithNewData(userReceived);
            ChangeUserReceivedStatusToUnauthenticated();
        }

        private void RemoveResources(Dictionary<ResourceKind, int> defaultResources)
        {
            foreach(ResourceKind resources in ResourcesConstants.resourcesName2.Keys)
            {
                defaultResources[resources] = 0;
            }
        }

        private UserDto UpdateAndReturnUser(UserDto newUserData, User userToUpdate)
        {
            if (RequestConstants.AdminIsAuthenticated) { 
               return UserConverter.ConvertToUserDto(_userRepository.UpdateUserResourcesQuantity(newUserData, userToUpdate.Id)); 
            }

            userToUpdate.Pseudo = newUserData.Pseudo != null
                ? newUserData.Pseudo
                : userToUpdate.Pseudo;

            User userModelToUpdate = _userRepository.ReplaceUserWithNewData(userToUpdate);

            return UserConverter.ConvertToUserDto(userModelToUpdate);
        }

        private void ChangeUserReceivedStatusToUnauthenticated()
            => RequestConstants.NewUserIsAuthenticated = false;

        private User CreateUserModelFromData(UserDto userData)
        {
            User newUser = CreateUserModelWithResources(userData);
            GiveUnitsToNewlyCreatedUser(newUser);
            return newUser;
        }

        /// <summary>
        /// Crée un nouvel utilisateur avec un pseudo, des ressources et une date de création sans ses vaisseaux.
        /// </summary>
        /// <param name="userData">Données de création d'utilisateur</param>
        /// <returns></returns>
        private User CreateUserModelWithResources(UserDto userData)
        {
            Dictionary<ResourceKind, int> userDefaultResources = GiveDefaultResourcesToCreatedUser();

            DateTimeOffset userDateOfCreation = userData.DateOfCreation == null
                ? DateTimeOffset.Now
                : DateTimeOffset.Parse(userData.DateOfCreation);

            return new User(userData.Id, userDateOfCreation, userDefaultResources) { Pseudo = userData.Pseudo };
        }

        /// <summary>
        /// Donne des ressources par defaut à un utilisateur à sa création
        /// </summary>
        /// <returns></returns>
        private Dictionary<ResourceKind, int> GiveDefaultResourcesToCreatedUser()
        {
            return new Dictionary<ResourceKind, int>()
            {
                { ResourceKind.Carbon, 20 },
                { ResourceKind.Iron, 10 },
                { ResourceKind.Gold, 0 },
                { ResourceKind.Aluminium, 0 },
                { ResourceKind.Titanium, 0 },
                { ResourceKind.Water, 50 },
                { ResourceKind.Oxygen, 50 }
            };
        }

        private T GetRandomEntityFromList<T>(List<T> entityList)
        {
            var random = new Random();
            int index = random.Next(entityList.Count);
            return entityList[index];
        }

        private void UpdateDatabaseUser(string userId, List<GenericUnit> units)
        {
            User userToUpdate = _userRepository.GetUser(userId);
            userToUpdate.Units = units;
            _userRepository.ReplaceUserWithNewData(userToUpdate);
        }

        private void UpdateSingleUnitForDatabaseUser(string userId, GenericUnit updatedUnitModel)
        {
            User userToUpdate = _userRepository.GetUser(userId);
            GenericUnit unitToUpdate = userToUpdate.Units.Find(unit => unit.Id == updatedUnitModel.Id);
            unitToUpdate = updatedUnitModel;
            UpdateDatabaseUser(userId, userToUpdate.Units);
        }
    }
}
