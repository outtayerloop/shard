using Shard.Shared.Core;
using Shard.WiemEtBrunelle.Web.Constants.Units;
using Shard.WiemEtBrunelle.Web.Models.Units;
using Shard.WiemEtBrunelle.Web.Models.Universe;
using Shard.WiemEtBrunelle.Web.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Shard.WiemEtBrunelle.Web.Services.Users
{
    public partial class UserService : ShardBaseEntityService, IUserService
    {

        public void UpdateResourcesQuantityForUser(string userId, Building buildingModel)
        {
            User user = _userRepository.GetUser(userId);
            Planet planet = GetSinglePlanetModelFromSystem(buildingModel.StarSystem, buildingModel.Planet);

            HandleResourceExtractionAccordingToType(buildingModel, user, planet);
            _sectorRepository.UpdateSystemWithNewData(GetSystemModelByName(buildingModel.StarSystem));
            _userRepository.ReplaceUserWithNewData(user);
        }

        private void HandleResourceExtractionAccordingToType(Building buildingModel, User user, Planet planet)
        {
            if (buildingModel.ResourceCategory == BuildingConstants.GaseousCategory)
            {
                ExtractOxygen(user, buildingModel, planet);
            }
            else if (buildingModel.ResourceCategory == BuildingConstants.LiquidCategory)
            {
                ExtractWater(user, buildingModel, planet);
            }
            else if (buildingModel.ResourceCategory == BuildingConstants.SolidCategory)
            {
                ExtractSolidResources(user, buildingModel, planet);
            }
        }

        private void ExtractSolidResources(User user, Building buildingModel, Planet planet)
        {
            int totalMinutes = GetTotalMinutesSinceLastExtraction(buildingModel);
            DateTime requestArrivalTime = _systemClockService.Now;

            if (BuildingCanExtractResources(buildingModel, requestArrivalTime))
            {
                UpdateMineExtraction(user, buildingModel, planet, totalMinutes, requestArrivalTime);
            }
        }

        private void UpdateMineExtraction(User user, Building buildingModel, Planet planet, int totalMinutes, DateTime requestArrivalTime)
        {
            for (int i = 0; i < totalMinutes; i++)
            {
                ProcessResourcesExtraction(user, planet);
            }

            buildingModel.LastMomentOfExtraction = requestArrivalTime;
        }

        private bool BuildingCanExtractResources(Building buildingModel, DateTime requestArrivalTime)
            => IsLastMomentOfExtractionDifferentFromNow(buildingModel, requestArrivalTime) && IsBuildingAchieved(buildingModel, requestArrivalTime);

        private bool IsLastMomentOfExtractionDifferentFromNow(Building buildingModel, DateTime requestArrivalTime)
            => buildingModel.LastMomentOfExtraction != requestArrivalTime;

        private bool IsBuildingAchieved(Building buildingModel, DateTime requestArrivalTime)
            => requestArrivalTime.CompareTo(buildingModel.BuildCompletionTime) > 0;

        private void ProcessResourcesExtraction(User user, Planet planet)
        {
            var resourceMostPresent = SelectTheResourceForExtraction(planet);

            GiveResourcesToUserAfterExtraction(user, planet, resourceMostPresent.resourceName);

            RemoveResourcesFromPlanetAfterExtraction(planet, resourceMostPresent.resourceName);
        }

        private (ResourceKind resourceName1, ResourceKind resourceName2) GetDuplicatesResourcesQuantityFromPlanet(Planet planet)
        {

            ResourceKind firstDuplicateResource = (ResourceKind)BuildingConstants.DefaultResource;
            ResourceKind secondDuplicateResource = (ResourceKind)BuildingConstants.DefaultResource;

            Dictionary<ResourceKind, int> solidResourcesOfPlanet = GetOnlySolidResourcesFromPlanet(planet);

            foreach (KeyValuePair<ResourceKind, int> resourceKind in solidResourcesOfPlanet)
            {
                (firstDuplicateResource, secondDuplicateResource) =  GetDuplicateResources(solidResourcesOfPlanet, resourceKind);
            }

            return (firstDuplicateResource, secondDuplicateResource);
        }

        private Dictionary<ResourceKind, int> GetOnlySolidResourcesFromPlanet(Planet planet)
        {
            var solidResourcesOnPlanet = new Dictionary<ResourceKind, int>();

            foreach (KeyValuePair<ResourceKind, int> kind in planet.ResourceQuantity)
            {
                if (BuildingConstants.SolidResources.Contains(kind.Key))
                {
                    solidResourcesOnPlanet.Add(kind.Key, kind.Value);
                }
            }

            return solidResourcesOnPlanet;
        }

        private (ResourceKind firstDuplicateResource, ResourceKind secondDuplicateResource) GetDuplicateResources(Dictionary<ResourceKind, int> solidResourcesOfPlanet, KeyValuePair<ResourceKind, int> resourceKind)
        {
            int resourceQuantity = resourceKind.Value;
            ResourceKind resourceName = resourceKind.Key;

            return CompareQuantityOfTwoResources(solidResourcesOfPlanet, resourceName, resourceQuantity);
        }

        private (ResourceKind duplicateResourceName1, ResourceKind duplicateResourceName2) CompareQuantityOfTwoResources(Dictionary<ResourceKind, int> solidResourcesOfPlanet, ResourceKind resourceName, int resourceQuantity)
        {
            foreach (KeyValuePair<ResourceKind, int> resource in solidResourcesOfPlanet)
            {
                if (TwoDifferentResourcesHaveSameQuantity(resourceName, resourceQuantity, resource))
                {
                    return (resourceName, resource.Key);
                }
            }

            return (resourceName, resourceName);
        }

        private bool TwoDifferentResourcesHaveSameQuantity(ResourceKind resourceName, int resourceQuantity, KeyValuePair<ResourceKind, int> resource)
            => resource.Value == resourceQuantity && resource.Key != resourceName;

        private (ResourceKind resourceName, int resourceQuantity) SelectTheResourceForExtraction(Planet planet)
        {

            Dictionary<ResourceKind, int> solidResourcesOfPlanet = GetOnlySolidResourcesFromPlanet(planet);

            int numberOfSolidResource = solidResourcesOfPlanet.Count;

            if (numberOfSolidResource == 1)
            {
                return ExtractOnlyOneResource(solidResourcesOfPlanet);
            }

            return AlternateBetweenResourceQuantityExtraction(planet, solidResourcesOfPlanet);

        }

        private (ResourceKind resourceName, int resourceQuantity) AlternateBetweenResourceQuantityExtraction(Planet planet, Dictionary<ResourceKind, int> solidResourcesOfPlanet)
        {
            (ResourceKind firstResourceName, ResourceKind secondResourceName) = GetDuplicatesResourcesQuantityFromPlanet(planet);

            if (firstResourceName == secondResourceName)
            {
                return GetMostPresentResourceOnPlanet(solidResourcesOfPlanet);
            }

            return GetMostRareResourceNameAndQuantity(planet, firstResourceName, secondResourceName);
        }

        private (ResourceKind resourceName, int resourceQuantity) GetMostRareResourceNameAndQuantity(Planet planet, ResourceKind firstResourceName, ResourceKind secondResourceName)
        {
            IReadOnlyDictionary<ResourceKind, int> planetResources = planet.ResourceQuantity;
            KeyValuePair<ResourceKind, int> mostRareResource = planetResources.OrderByDescending(resource => resource.Value).Last();
            
            return (mostRareResource.Key, mostRareResource.Value);
        }

        private (ResourceKind resourceName, int resourceQuantity) GetMostPresentResourceOnPlanet(Dictionary<ResourceKind, int> solidResourcesOfPlanet)
        {
            KeyValuePair<ResourceKind, int> mostPresentResource = solidResourcesOfPlanet.OrderByDescending(resource => resource.Value).First();
            return (mostPresentResource.Key, mostPresentResource.Value);
        }

        private (ResourceKind resourceName, int resourceQuantity) ExtractOnlyOneResource(Dictionary<ResourceKind,int> solidResourcesOfPlanet)
        {
            int numberOfSolidResource = solidResourcesOfPlanet.Count;

            if (numberOfSolidResource == 1)
            {
                ResourceKind uniqueSolidResourceName = solidResourcesOfPlanet.FirstOrDefault().Key;
                int uniqueSolidResourceValue = solidResourcesOfPlanet.FirstOrDefault().Value;
                return (uniqueSolidResourceName, uniqueSolidResourceValue);
            }
            return ((ResourceKind)(BuildingConstants.DefaultResource), 0);
        }

        private void ExtractOxygen(User user, Building buildingModel, Planet planet)
        {

            int totalMinutes = GetTotalMinutesSinceLastExtraction(buildingModel);

            for (int i = 0; i < totalMinutes; i++)
            {
                GiveResourcesToUserAfterExtraction(user, planet, ResourceKind.Oxygen);

                RemoveResourcesFromPlanetAfterExtraction(planet, ResourceKind.Oxygen);

            }
        }

        private int GetTotalMinutesSinceLastExtraction(Building buildingModel)
        {
            DateTime requestArrivalTime = _systemClockService.Now;

            int totalMinutesSinceLastExtraction = GetMinutesSinceLastExtractionTime(buildingModel, requestArrivalTime);

            return totalMinutesSinceLastExtraction < 0 
                ? requestArrivalTime.Minute
                : totalMinutesSinceLastExtraction;
        }

        private int GetMinutesSinceLastExtractionTime(Building buildingModel, DateTime requestArrivalTime)
        {
            return GetMinutesFromBuildingLastExtractionHourComparedToNow(buildingModel, requestArrivalTime) 
                + GetMinutesFromBuildingLastExtractionTimeInMinutesComparedToNow(buildingModel, requestArrivalTime);
        }

        private int GetMinutesFromBuildingLastExtractionHourComparedToNow(Building buildingModel, DateTime requestArrivalTime)
            => requestArrivalTime.Subtract(buildingModel.LastMomentOfExtraction).Hours * 60;

        private int GetMinutesFromBuildingLastExtractionTimeInMinutesComparedToNow(Building buildingModel, DateTime requestArrivalTime)
            => requestArrivalTime.Subtract(buildingModel.LastMomentOfExtraction).Minutes;

        private void ExtractWater(User user, Building buildingModel, Planet planet)
        {

            int totalMinutes = GetTotalMinutesSinceLastExtraction(buildingModel);

            for (int i = 0; i < totalMinutes; i++)
            {

                GiveResourcesToUserAfterExtraction(user, planet, ResourceKind.Water);

                RemoveResourcesFromPlanetAfterExtraction(planet, ResourceKind.Water);

            }
        }

        private void GiveResourcesToUserAfterExtraction(User user, Planet planet, ResourceKind resourceKind)
        {
            user.ResourcesQuantity[resourceKind] += planet.ResourceQuantity[resourceKind] > 0
                ? 1
                : 0;
        }

        private void RemoveResourcesFromPlanetAfterExtraction(Planet planet, ResourceKind resourceKind)
        {
            Dictionary<ResourceKind, int> resourcesPlanet = planet.ResourceQuantity;

            if (planet.ResourceQuantity.ContainsKey(resourceKind))
            {
                resourcesPlanet[resourceKind] -= planet.ResourceQuantity[resourceKind] > 0 ? 1 : 0;
            }

            planet = new Planet(planet.Name, planet.Size, resourcesPlanet);
        }

    }
}
