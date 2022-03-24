using Shard.WiemEtBrunelle.Web.Constants.Units;
using Shard.WiemEtBrunelle.Web.Converters;
using Shard.WiemEtBrunelle.Web.Dto.Units;
using Shard.WiemEtBrunelle.Web.Models.Units;
using Shard.WiemEtBrunelle.Web.Models.Users;
using Shard.WiemEtBrunelle.Web.Utils.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Shard.WiemEtBrunelle.Web.Services.Users
{
    public partial class UserService : ShardBaseEntityService, IUserService
    {
        public BuildingDto OrderBuildingCreation(BuildingDto buildingData, UnitDto unitBuilder)
        {
            Building buildingModel = CreateBuildingModelFromData(buildingData, unitBuilder);

            _buildingRepository.AddBuilding(buildingModel);
           
            return BuildingConverter.ConvertToBuildingDto(buildingModel);
        }

        public List<BuildingDto> GetAllBuildingsFromUser(string userId)
        {

            GenericUnit builderUnit = GetUserBuildingBuilder(userId);

            if (builderUnit == null) 
                return null;

            List<Building> userBuildings = _buildingRepository.GetAllBuildingsFromBuilder(builderUnit.Id);

            if (userBuildings == null) 
                return new List<BuildingDto>() { null };

            return GetBuildingDtoListFromBuildingModelList(userBuildings);
        }

        public async Task<BuildingDto> GetSingleBuildingFromUser(string userId, string buildingId)
        {
            GenericUnit builderUnit = GetUserBuildingBuilder(userId);

            if (builderUnit == null)
                return null;

            Building buildingModel = _buildingRepository.GetSingleBuildingFromBuilder(builderUnit.Id, buildingId);

            if (buildingModel == null)
                return null;

            return await GetUpdatedBuildingDto(buildingId, builderUnit, buildingModel);
        }

        private async Task<BuildingDto> GetUpdatedBuildingDto(string buildingId, GenericUnit builderUnit, Building buildingModel)
        {
            await CheckAndUpdateBuildingAchievementData(_systemClockService.Now, buildingModel, TimeTiedEntityUpdateContext.WaitForEntityActionCompletion);

            return CheckForBuildingExistenceAndReturnFoundBuildingDto(builderUnit, buildingId);
        }

        private BuildingDto CheckForBuildingExistenceAndReturnFoundBuildingDto(GenericUnit builderUnit, string buildingId)
        {
            Building buildingModel = _buildingRepository.GetSingleBuildingFromBuilder(builderUnit.Id, buildingId);

            if (buildingModel == null) 
                return new BuildingDto(null, null);

            return BuildingConverter.ConvertToBuildingDto(buildingModel);
        }

        private async Task CheckAndUpdateBuildingAchievementData(DateTime requestArrivalTime, Building buildingModel, TimeTiedEntityUpdateContext updateContext)
        {
            if (!buildingModel.IsBuilt)
            {
                await HandleBuildingInProgressState(requestArrivalTime, buildingModel, updateContext);
            }
        }

        private async Task HandleBuildingInProgressState(DateTime requestArrivalTime, Building buildingModel, TimeTiedEntityUpdateContext updateContext)
        {
            if (PeekRequestArrivedOnOrAfterBuildingAchievementTime(requestArrivalTime, buildingModel))
            {
                AchieveBuilding(buildingModel);
            }
            else
            {
                await UpdateBuildingTimeData(requestArrivalTime, buildingModel, updateContext);
            }
        }

        private bool PeekRequestArrivedOnOrAfterBuildingAchievementTime(DateTime requestArrivalTime, Building buildingModel)
            => requestArrivalTime.CompareTo(buildingModel.EstimatedBuildTime) >= 0;

        private async Task UpdateBuildingTimeData(DateTime requestArrivalTime, Building buildingModel, TimeTiedEntityUpdateContext updateContext)
        {
            TimeSpan remainingTimeBeforeBuilt = GetUpdatedEstimatedTimeOfAction(buildingModel.EstimatedBuildTime.Value, requestArrivalTime);

            if (MustWaitBuildingAchievement(remainingTimeBeforeBuilt, updateContext))
            {
                await HandleBuildingAchievementWaiting(buildingModel, remainingTimeBeforeBuilt);
            }
        }

        private async Task HandleBuildingAchievementWaiting(Building buildingModel, TimeSpan remainingTimeBeforeBuilt)
        {
            Task buildingAchievementWaitingTask = GetBuildingAchievementWaitingTask(remainingTimeBeforeBuilt);

            _tasksBag.Add(buildingAchievementWaitingTask);

            await TryToWaitBuildingAchievement(buildingModel, buildingAchievementWaitingTask);
        }

        private Task GetBuildingAchievementWaitingTask(TimeSpan remainingTimeBeforeBuilt)
        {
            CancellationToken cancellationToken = _cancellationTokenSource.Token;

            return Task.Run(() => WaitForEntityActionAchievement(remainingTimeBeforeBuilt, cancellationToken), cancellationToken);
        }

        private async Task TryToWaitBuildingAchievement(Building buildingModel, Task buildingAchievementWaitingTask)
        {
            try
            {
                await buildingAchievementWaitingTask;
                AchieveBuilding(buildingModel);
            }
            catch (OperationCanceledException)
            {
                CancelBuildingBuildAndRemoveFromList(buildingModel);
            }
            finally
            {
                DisposeCancellationTokenResources();
            }
        }

        private void CancelBuildingBuildAndRemoveFromList(Building buildingModel)
            => _buildingRepository.RemoveBuilding(buildingModel);

        private void DisposeCancellationTokenResources()
        {
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        private bool MustWaitBuildingAchievement(TimeSpan remainingTimeBeforeBuilt, TimeTiedEntityUpdateContext updateContext)
            => IsTimeTiedEntityActionWaitingContext(updateContext) && IsBuildingAchievementWaitingTime(remainingTimeBeforeBuilt);

        private bool IsBuildingAchievementWaitingTime(TimeSpan remainingTimeBeforeBuilt)
            => remainingTimeBeforeBuilt.CompareTo(TimeSpan.FromSeconds(BuildingConstants.RemainingSecondsLimitForRequestAwait)) <= 0;

        private void AchieveBuilding(Building buildingModel)
        {
            buildingModel.IsBuilt = true;
            buildingModel.EstimatedBuildTime = null;
            _buildingRepository.ReplaceBuildingFromData(buildingModel);
        }

        private Building CreateBuildingModelFromData(BuildingDto buildingData, UnitDto unitBuilder)
        {

            if (IsValidCategoryResource(buildingData) && IsUnitBuilder(unitBuilder) && IsValidBuildingType(buildingData))
            {
                Building buildingModel = GetNewlyCreatedBuildingModel(buildingData, unitBuilder);

                if (buildingData.Type == BuildingConstants.BuildingConstructionType)
                    RemoveResourceCategoryForBuildingConstruction(buildingModel);

                return buildingModel;
            }

            return null;
        }

        private Building GetNewlyCreatedBuildingModel(BuildingDto buildingData, UnitDto unitBuilder)
        {
            return new Building()
            {
                Id = Guid.NewGuid().ToString(),
                Type = buildingData.Type,
                ResourceCategory = buildingData.ResourceCategory,
                BuilderId = unitBuilder.Id,
                StarSystem = unitBuilder.System,
                Planet = unitBuilder.Planet,
                IsBuilt = false,
                EstimatedBuildTime = _systemClockService.Now.AddMinutes(5),
                BuildCompletionTime = _systemClockService.Now.AddMinutes(5),
                LastMomentOfExtraction = _systemClockService.Now.AddMinutes(5),
                UnitsQueue = new List<GenericUnit>()
            };
        }

        private void RemoveResourceCategoryForBuildingConstruction(Building building)
            => building.ResourceCategory = null;

        private List<BuildingDto> GetBuildingDtoListFromBuildingModelList(List<Building> userBuildings)
        {
            var buildingDtoList = new List<BuildingDto>();
            userBuildings.ForEach(building => buildingDtoList.Add(BuildingConverter.ConvertToBuildingDto(building)));
            return buildingDtoList;
        }

        private GenericUnit GetUserBuildingBuilder(string userId)
        {
            User user = _userRepository.GetUser(userId);

            if (user == null) 
                return null;

            return user.Units.Where(unit => unit.UnitType == UnitConstants.BuilderType).First();
        }

        private void CheckForUnitBuildingsProgressContinuity(string builderId, string builderDestinationSystem,
            string builderSystem, string builderDestinationPlanet, string builderPlanet)
        {
            List<Building> builderBuildings = _buildingRepository.GetAllBuildingsFromBuilder(builderId);

            builderBuildings.ForEach(building =>
            {
                HandleBuildingsToRemove(builderDestinationSystem, builderSystem, builderDestinationPlanet, builderPlanet, building);
            });
        }

        private void HandleBuildingsToRemove(string builderDestinationSystem, string builderSystem, string builderDestinationPlanet, string builderPlanet, Building building)
        {
            if (IsBuildingToRemove(builderDestinationSystem, builderSystem, builderDestinationPlanet, builderPlanet, building))
            {
                _buildingRepository.RemoveBuilding(building);

                SeekTasksToCancelAndCancelThem();
            }
        }

        private void SeekTasksToCancelAndCancelThem()
        {
            if (TasksBagContainsPendingTasks())
                _cancellationTokenSource.Cancel();
        }

        private bool IsUnitBuilder(UnitDto unitBuilder)
            => unitBuilder.Type == UnitConstants.BuilderType;

        private bool IsValidCategoryResource(BuildingDto building)
            => BuildingConstants.resourcesCategory.Contains(building.ResourceCategory);

        private bool IsValidBuildingType(BuildingDto building)
            => BuildingConstants.BuildingType.Contains(building.Type);

        private bool TasksBagContainsPendingTasks()
            => _tasksBag.Where(task => task.Status != TaskStatus.Canceled).ToList().Count > 0;

        private bool IsBuildingToRemove(string builderDestinationSystem, string builderSystem, 
            string builderDestinationPlanet, string builderPlanet, Building building)
        {
            return IsNotBuiltYetAndWillBeAborted(building, builderSystem, builderPlanet) &&
                !IsFakeMove(builderDestinationSystem, builderSystem, builderDestinationPlanet, builderPlanet);
        }

        private bool IsNotBuiltYetAndWillBeAborted(Building building, string builderSystem, string builderPlanet)
            => IsBuildingPartOfBuilderLocationPlanet(building, builderSystem, builderPlanet) && !building.IsBuilt;

        private bool IsBuildingPartOfBuilderLocationPlanet(Building building, string builderSystem, string builderPlanet)
            => building.StarSystem == builderSystem && building.Planet == builderPlanet;

        private bool IsFakeMove(string unitDestinationSystem, string unitSystem, string unitDestinationPlanet, string unitPlanet)
            => unitDestinationSystem == unitSystem && unitDestinationPlanet == unitPlanet;

    }
}
