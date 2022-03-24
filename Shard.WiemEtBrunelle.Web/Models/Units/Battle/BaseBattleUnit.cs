using MongoDB.Bson.Serialization.Attributes;
using Shard.WiemEtBrunelle.Web.Constants.Units;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Shard.WiemEtBrunelle.Web.Models.Units.Battle
{
    [BsonKnownTypes(typeof(FighterUnit), typeof(CruiserUnit), typeof(BomberUnit))]
    public abstract class BaseBattleUnit : GenericUnit
    {

        public BaseBattleUnit(string id) : base(id) { }

        public override int Health { get; set; }

        public int DamageTaken { get; set; }

        public override bool CanScanPlanetResources => false;

        public virtual int AttackPower => 0;

        public virtual int CanonNumber => 0;

        public virtual int FiringPeriodInSeconds => 0;

        public virtual List<string> OrderedPrimaryTargets => null;

        public void ShootEnemyUnitsByPriority(List<BaseBattleUnit> localEnemyUnitList, DateTime currentDateTime)
        {
            int shootNumber = GetShootNumber(currentDateTime);

            for (int i = 0; i < shootNumber; ++i)
            {
                HandleTargetsToShoot(localEnemyUnitList);
            }
        }

        private int GetShootNumber(DateTime currentDateTime)
        {
            return UnitType == UnitConstants.BomberType
                ? GetMinutesFromCurrentDateTime(currentDateTime)
                : GetShootNumberForBattleUnitsOtherThanBomber(currentDateTime);
        }

        private int GetShootNumberForBattleUnitsOtherThanBomber(DateTime currentDateTime)
        {
            TimeSpan elapsedTimeSinceArrival = UnitHasReachedDestination()
                            ? currentDateTime.Subtract(UnitDestination.DateOfArrival)
                            : currentDateTime.Subtract(UnitDestination.DateOfEntryInNewSystem);

            return GetElapsedSecondsSinceArrival(elapsedTimeSinceArrival) / FiringPeriodInSeconds;
        }

        private int GetMinutesFromCurrentDateTime(DateTime currentDateTime)
            => currentDateTime.Hour * 60 +
                currentDateTime.Minute;

        private int GetElapsedSecondsSinceArrival(TimeSpan elapsedTimeSinceArrival)
            =>  elapsedTimeSinceArrival.Days * 3600 * 24 +
                elapsedTimeSinceArrival.Hours * 3600 +
                elapsedTimeSinceArrival.Minutes * 60 +
                elapsedTimeSinceArrival.Seconds;

        private void HandleTargetsToShoot(List<BaseBattleUnit> localEnemyUnitList)
        {
            BaseBattleUnit enemyToShoot = GetNextTarget(localEnemyUnitList);

            if (enemyToShoot != null)
            {
                InflictDamageToTarget(enemyToShoot);
            }
        }

        private BaseBattleUnit GetNextTarget(List<BaseBattleUnit> enemyUnitList)
        {
            bool isNextTargetFound = false;
            BaseBattleUnit nextTarget = null;

            for (int i = 0; i < OrderedPrimaryTargets.Count && !isNextTargetFound; ++i)
            {
                nextTarget = enemyUnitList.Where(enemyUnit => IsNextTarget(enemyUnit, i)).FirstOrDefault();
                isNextTargetFound = nextTarget != null;
            }

            return nextTarget;
        }

        private bool IsNextTarget(BaseBattleUnit enemyUnit, int i)
            => enemyUnit.UnitType == OrderedPrimaryTargets[i] && enemyUnit.Health > 0;

        private void InflictDamageToTarget(BaseBattleUnit enemyToShoot)
        {
            int damageInflicted = IsDamageGoingToBeDeflected(enemyToShoot)
                ? (AttackPower * CanonNumber) / UnitConstants.BomberDeflectDivisor
                : AttackPower * CanonNumber;

            enemyToShoot.DamageTaken += damageInflicted;
        }

        private bool IsDamageGoingToBeDeflected(BaseBattleUnit enemyToShoot)
            => UnitType == UnitConstants.CruiserType && enemyToShoot.UnitType == UnitConstants.BomberType;
    }
}
