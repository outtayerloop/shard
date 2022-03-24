using Shard.WiemEtBrunelle.Web.Constants.Units;
using Shard.WiemEtBrunelle.Web.Models.Units;
using Shard.WiemEtBrunelle.Web.Models.Universe;
using Shard.WiemEtBrunelle.Web.Utils.Units;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Shard.WiemEtBrunelle.Web.Services.Users
{
    public partial class UserService : ShardBaseEntityService, IUserService
    {

        private int GetUnitSecondsBeforeArrival(GenericUnit unitModelToUpdate, StarSystem newDestinationSystemModel)
        {
            return IsAlreadyInDestinationSystem(unitModelToUpdate, newDestinationSystemModel) 
                ? UnitConstants.SecondsToEnterPlanet 
                : UnitConstants.SecondsToEnterPlanet + UnitConstants.SecondsToReachNewSystem;
        }

        private void UpdateUnitLocationAtMoveOrder(GenericUnit unit, string newDestinationSystemName, string newDestinationPlanetName)
        {

            if(unit.UnitType == UnitConstants.BuilderType)
            {
                CheckForUnitBuildingsProgressContinuity(unit.Id, newDestinationSystemName, unit.UnitLocation.StarSystem.Name,
                    newDestinationPlanetName, unit.UnitLocation.Planet?.Name);
            }

            unit.UnitLocation.Planet = null;
        }

        private bool IsAlreadyInDestinationSystem(GenericUnit unit, StarSystem destinationSystem)
            => destinationSystem.Name == unit.UnitLocation.StarSystem.Name;

        private async Task<GenericUnit> UpdateUnitSpaceTimeData(GenericUnit foundUnit,
            TimeTiedEntityUpdateContext updateContext, DateTime requestArrivalDate)
        {

            DateTime unitDateOfArrival = foundUnit.UnitDestination.DateOfArrival;

            await HandleUnitSpaceTimeDataAndContext(foundUnit, updateContext, requestArrivalDate, unitDateOfArrival);

            return foundUnit;

        }

        private async Task HandleUnitSpaceTimeDataAndContext(GenericUnit foundUnit, TimeTiedEntityUpdateContext updateContext, DateTime requestArrivalDate, DateTime unitDateOfArrival)
        {
            if (RequestArrivalDateIsLaterThanUnitArrivalDate(requestArrivalDate, unitDateOfArrival))
            {
                UnitReachesDestination(foundUnit);
            }
            else
            {
                await CheckAndUpdateUnitArrivalStatus(foundUnit, updateContext, requestArrivalDate, unitDateOfArrival);
            }
        }

        private bool RequestArrivalDateIsLaterThanUnitArrivalDate(DateTime requestArrivalDate, DateTime unitDateOfArrival)
            => unitDateOfArrival.CompareTo(requestArrivalDate) == 0 || unitDateOfArrival.CompareTo(requestArrivalDate) < 0;

        private async Task CheckAndUpdateUnitArrivalStatus(GenericUnit foundUnit, TimeTiedEntityUpdateContext updateContext, DateTime requestArrivalDate, DateTime unitDateOfArrival)
        {
            TimeSpan unitEstimatedTimeOfArrival = GetUpdatedEstimatedTimeOfAction(unitDateOfArrival, requestArrivalDate);
            foundUnit.UnitDestination.EstimatedTimeOfArrival = unitEstimatedTimeOfArrival;
            await UpdateUnitLocationFromEstimatedTimeOfArrival(foundUnit, updateContext, unitEstimatedTimeOfArrival);
        }

        private TimeSpan GetUpdatedEstimatedTimeOfAction(DateTime actionDateOfAchievement, DateTime requestArrivalDate)
        {
            var unitArrivalTimeSpan = new TimeSpan(0, actionDateOfAchievement.Minute, actionDateOfAchievement.Second);
            var requestArrivalTimeSpan = new TimeSpan(0, requestArrivalDate.Minute, requestArrivalDate.Second);

            return unitArrivalTimeSpan.Subtract(requestArrivalTimeSpan);
        }

        private async Task UpdateUnitLocationFromEstimatedTimeOfArrival(GenericUnit foundUnit, TimeTiedEntityUpdateContext updateContext,
            TimeSpan unitEstimatedTimeOfArrival)
        {
            if (UnitEstimatedTimeOfArrivalIsLessThanOneMinute(unitEstimatedTimeOfArrival))
            {
                foundUnit.UnitLocation.StarSystem = foundUnit.UnitDestination.StarSystem;
                foundUnit.UnitDestination.DateOfEntryInNewSystem = _systemClockService.Now;
                await WaitForUnitArrivalConditionally(foundUnit, updateContext, unitEstimatedTimeOfArrival);
            }
        }

        private bool UnitEstimatedTimeOfArrivalIsLessThanOneMinute(TimeSpan unitEstimatedTimeOfArrival)
            => unitEstimatedTimeOfArrival.CompareTo(TimeSpan.FromMinutes(1)) < 0;

        /// <summary>
        /// Si le contexte le permet et si le temps restant avant arrivée à destination du vaisseau est inférieur
        /// ou égal à la limite de temps fixée, attend que le vaisseau arrive destination, modifie sa position et
        /// lui donne un temps restant avant arrivée nul.
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="updateContext"></param>
        /// <param name="unitEstimatedTimeOfArrival"></param>
        /// <returns></returns>
        private async Task WaitForUnitArrivalConditionally(GenericUnit unit, TimeTiedEntityUpdateContext updateContext,
            TimeSpan unitEstimatedTimeOfArrival)
        {
            if (MustWaitUnitArrival(updateContext, unitEstimatedTimeOfArrival))
            {
                await WaitForEntityActionAchievement(unitEstimatedTimeOfArrival);
                UnitReachesDestination(unit);
            }
        }

        /// <summary>
        /// Détermine s'il faut ou non attendre que le vaisseau arrive à destination.
        /// </summary>
        /// <param name="updateContext"></param>
        /// <param name="unitEstimatedTimeOfArrival"></param>
        /// <returns></returns>
        private bool MustWaitUnitArrival(TimeTiedEntityUpdateContext updateContext, TimeSpan unitEstimatedTimeOfArrival)
            => IsTimeTiedEntityActionWaitingContext(updateContext) && IsUnitArrivalWaitingTime(unitEstimatedTimeOfArrival);

        private bool IsTimeTiedEntityActionWaitingContext(TimeTiedEntityUpdateContext updateContext)
            => updateContext == TimeTiedEntityUpdateContext.WaitForEntityActionCompletion;

        private bool IsUnitArrivalWaitingTime(TimeSpan unitEstimatedTimeOfArrival)
            => unitEstimatedTimeOfArrival.Seconds <= UnitConstants.RemainingSecondsLimitForRequestAwait;

        /// <summary>
        /// Attend que l'action concernée se termine.
        /// </summary>
        /// <param name="remainingTimeToWait"></param>
        /// <returns></returns>
        private async Task WaitForEntityActionAchievement(TimeSpan remainingTimeToWait)
            => await _systemClockService.Delay(remainingTimeToWait);

        /// <summary>
        /// Attend que l'action concernée se termine et peut être annulée par une demande extérieure.
        /// </summary>
        /// <param name="remainingTimeToWait"></param>
        /// <returns></returns>
        private async Task WaitForEntityActionAchievement(TimeSpan remainingTimeToWait, CancellationToken cancellationToken)
            => await _systemClockService.Delay(remainingTimeToWait, cancellationToken);

        private void UnitReachesDestination(GenericUnit unit)
            => unit.ReachDestination();

    }
}
