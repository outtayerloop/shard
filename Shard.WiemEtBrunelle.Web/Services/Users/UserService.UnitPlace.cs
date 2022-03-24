using Shard.WiemEtBrunelle.Web.Models.Units;
using Shard.WiemEtBrunelle.Web.Models.Units.GeographicData;
using Shard.WiemEtBrunelle.Web.Models.Universe;
using System;
using System.Collections.Generic;

namespace Shard.WiemEtBrunelle.Web.Services.Users
{
    public partial class UserService : ShardBaseEntityService, IUserService
    {
        /// <summary>
        /// Crée une localisation aléatoire pour un vaisseau à partir d'un système,
        /// celle-ci comportant ou non des ressources en fonction de la capacité du vaisseau à scanner celles d'une planète.
        /// </summary>
        /// <param name="canScanPlanetResources"></param>
        /// <param name="system"></param>
        /// <returns></returns>
        public BaseUnitLocation GetInitialUnitLocation(bool canScanPlanetResources, StarSystem system)
        {
            Planet planet = null;

            BaseUnitLocation randomLocation = GetLocationInstanceConditionally(canScanPlanetResources);
            randomLocation.StarSystem = system;
            randomLocation.Planet = planet;

            return randomLocation;
        }

        /// <summary>
        /// Crée une instance d'un sous-Type de BaseUnitLocation selon que le unit passé soit capable de scanner
        /// les ressources d'une planète ou non.
        /// </summary>
        /// <param name="canScanPlanetResources"></param>
        /// <returns></returns>
        private BaseUnitLocation GetLocationInstanceConditionally(bool canScanPlanetResources)
        {
            if (canScanPlanetResources)
                return new UnitLocationWithResources();

            return new ResourcelessUnitLocation();
        }

        private void AssociateInitialUnitDestination(List<GenericUnit> units)
        {
            units.ForEach(unit =>
            {
                StarSystem system = unit.UnitLocation.StarSystem;
                Planet planet = unit.UnitLocation.Planet;
                unit.UnitDestination = GetInitialUnitDestination(system, planet);
            });
        }

        private UnitDestination GetInitialUnitDestination(StarSystem system, Planet planet)
        {
            return new UnitDestination()
            {
                DateOfArrival = _systemClockService.Now,
                EstimatedTimeOfArrival = new TimeSpan(0, 0, 0),
                StarSystem = system,
                Planet = planet,
                DateOfEntryInNewSystem = _systemClockService.Now
            };
        }

        private UnitDestination GetUpdatedUnitDestination(GenericUnit unit, StarSystem destinationSystem, Planet destinationPlanet)
        {
            int secondsBeforeArrival = GetUnitSecondsBeforeArrival(unit, destinationSystem);

            return new UnitDestination()
            {
                StarSystem = destinationSystem,
                Planet = destinationPlanet,
                DateOfArrival = _systemClockService.Now.AddSeconds(secondsBeforeArrival),
                EstimatedTimeOfArrival = new TimeSpan(0, 0, secondsBeforeArrival)
            };
        }
    }
}
