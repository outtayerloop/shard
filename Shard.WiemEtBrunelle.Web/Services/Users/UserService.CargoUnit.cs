using Shard.Shared.Core;
using Shard.WiemEtBrunelle.Web.Constants.Resources;
using Shard.WiemEtBrunelle.Web.Dto.Units;
using Shard.WiemEtBrunelle.Web.Models.Units;
using Shard.WiemEtBrunelle.Web.Models.Users;
using System.Collections.Generic;
using System.Linq;

namespace Shard.WiemEtBrunelle.Web.Services.Users
{
    public partial class UserService
    {
        private GenericUnit LoadOrUnloadResourcesInCargo(UnitDto newUnitDataDto, GenericUnit unitModelToUpdate, string userId)
        {

            InitializeResourceQuantityForUnit(unitModelToUpdate);

            User user = _userRepository.GetUser(userId);

            if (NoChangingResourcesQuantity(newUnitDataDto, unitModelToUpdate))
                return unitModelToUpdate;

            if (!IsThereStarportOnPlanet(unitModelToUpdate))
                return null;
            
            if (HaveToLoadResources(newUnitDataDto, unitModelToUpdate))
                return LoadResourcesInCargo(newUnitDataDto, unitModelToUpdate, user);

            return UnLoadResourcesInCargo(newUnitDataDto, unitModelToUpdate, user);
    
        }

        private bool HaveToLoadResources(UnitDto newUnitDataDto, GenericUnit unitModelToUpdate)
            => newUnitDataDto.ResourcesQuantity.Values.Sum() > unitModelToUpdate.ResourcesQuantity.Values.Sum();
        
        private void InitializeResourceQuantityForUnit(GenericUnit unitModelToUpdate)
        {
            unitModelToUpdate.ResourcesQuantity =
                 unitModelToUpdate.ResourcesQuantity == null ?
                 new Dictionary<ResourceKind, int>() { } : unitModelToUpdate.ResourcesQuantity;
        }

        private bool NoChangingResourcesQuantity(UnitDto newUnitDataDto, GenericUnit unitModelToUpdate)
        {
            bool changes = true;

            GiveDefaultResourcesToUnit(unitModelToUpdate);

            foreach (string resources in newUnitDataDto.ResourcesQuantity.Keys)
            {
                if (newUnitDataDto.ResourcesQuantity[resources] != unitModelToUpdate.ResourcesQuantity[ResourcesConstants.resourcesName[resources]])
                    changes = false;
            }

            return changes;
        }

        private void GiveDefaultResourcesToUnit(GenericUnit unitModelToUpdate)
        {
            Dictionary<ResourceKind, int> defaultResources = new Dictionary<ResourceKind, int>()
            {
                { ResourceKind.Carbon,  0},
                { ResourceKind.Iron, 0},
                { ResourceKind.Gold, 0},
                { ResourceKind.Aluminium, 0},
                {  ResourceKind.Titanium, 0 },
                {  ResourceKind.Water, 0},
                {  ResourceKind.Oxygen, 0}
            };

            unitModelToUpdate.ResourcesQuantity = unitModelToUpdate.ResourcesQuantity.Count == 0
                ? defaultResources
                : unitModelToUpdate.ResourcesQuantity;
        }

        private bool IsThereStarportOnPlanet(GenericUnit unitModelToUpdate)
        {
            if (unitModelToUpdate.UnitLocation.Planet != null)
                return unitModelToUpdate.UnitLocation.Planet.Starports != null;

            return false;

        }

        private GenericUnit LoadResourcesInCargo(UnitDto newUnitDataDto, GenericUnit unitModelToUpdate, User user)
        {
           
            GiveDefaultResourcesToUnit(unitModelToUpdate);

            foreach (string resource in newUnitDataDto.ResourcesQuantity.Keys)
            {

                if (UserHasEnoughResourcesForCargoLoading(user, newUnitDataDto, resource))
                {
                    LoadingOneResourceToUnitModel(unitModelToUpdate, newUnitDataDto, resource);
                    LoadingOneResourceCosts(user, newUnitDataDto, resource);
                    _userRepository.ReplaceUserWithNewData(user);
                }
                 
                else
                    return null;
            }

            return unitModelToUpdate;
        }

        private void LoadingOneResourceToUnitModel(GenericUnit unitModelToUpdate, UnitDto newUnitDataDto, string resource)
            => unitModelToUpdate.ResourcesQuantity[ResourcesConstants.resourcesName[resource]] += newUnitDataDto.ResourcesQuantity[resource];

        private void LoadingOneResourceCosts(User user, UnitDto newUnitDataDto, string resource)
            => user.ResourcesQuantity[ResourcesConstants.resourcesName[resource]] -= newUnitDataDto.ResourcesQuantity[resource];


        private bool UserHasEnoughResourcesForCargoLoading(User user, UnitDto newUnitDataDto, string resource)
            => user.ResourcesQuantity[ResourcesConstants.resourcesName[resource]] >= newUnitDataDto.ResourcesQuantity[resource];

        private GenericUnit UnLoadResourcesInCargo(UnitDto newUnitDataDto, GenericUnit unitModelToUpdate, User user)
        {

            foreach (string resource in newUnitDataDto.ResourcesQuantity.Keys)
            {
                LoadingOneResourceToUser(user, unitModelToUpdate, newUnitDataDto, resource);
                UnLoadingOneResourceToUnit(unitModelToUpdate, newUnitDataDto, resource);
                _userRepository.ReplaceUserWithNewData(user);
            }

            return unitModelToUpdate;
        }

        private void LoadingOneResourceToUser(User user, GenericUnit unitModelToUpdate, UnitDto newUnitDataDto, string resource)
            => user.ResourcesQuantity[ResourcesConstants.resourcesName[resource]] += (unitModelToUpdate.ResourcesQuantity[ResourcesConstants.resourcesName[resource]] - newUnitDataDto.ResourcesQuantity[resource]);

        private void UnLoadingOneResourceToUnit(GenericUnit unitModelToUpdate, UnitDto newUnitDataDto, string resource)
            => unitModelToUpdate.ResourcesQuantity[ResourcesConstants.resourcesName[resource]] = newUnitDataDto.ResourcesQuantity[resource];

    }
}
