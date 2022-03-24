using Shard.WiemEtBrunelle.Web.Constants.Units;
using Shard.WiemEtBrunelle.Web.Models.Units;
using Shard.WiemEtBrunelle.Web.Models.Units.Battle;
using Shard.WiemEtBrunelle.Web.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Shard.WiemEtBrunelle.Web.Services.Users
{
    public partial class UserService : ShardBaseEntityService, IUserService
    {
        private GenericUnit HandleBattlesBetweenEnemyUnits(string userId, GenericUnit battleUnit)
        {
            DateTime requestArrivalTime = _systemClockService.Now;

            BaseBattleUnit currentBattleUnit = GetBattleUnitFromGenericUnit(battleUnit);
            
            List<BaseBattleUnit> enemyBattleUnitList = GetLocalEnemyUnitList(userId, currentBattleUnit);

            return MakeEnemyUnitsShootEachOther(requestArrivalTime, currentBattleUnit, enemyBattleUnitList);
        }

        private BaseBattleUnit MakeEnemyUnitsShootEachOther(DateTime requestArrivalTime, BaseBattleUnit currentBattleUnit, List<BaseBattleUnit> enemyBattleUnitList)
        {
            ConductRoundOfFire(requestArrivalTime, currentBattleUnit, enemyBattleUnitList);

            RemoveDestroyedUnits(enemyBattleUnitList);

            return currentBattleUnit;
        }

        private void ConductRoundOfFire(DateTime requestArrivalTime, BaseBattleUnit currentBattleUnit, List<BaseBattleUnit> enemyBattleUnitList)
        {
            enemyBattleUnitList.ForEach(enemyUnit =>
            {
                List<BaseBattleUnit> currentEnemyUnitList = GetAllEnemyBattleUnitsForSingleEnemy(currentBattleUnit, enemyBattleUnitList, enemyUnit);
                enemyUnit.ShootEnemyUnitsByPriority(currentEnemyUnitList, requestArrivalTime);
            });
        }

        private List<BaseBattleUnit> GetAllEnemyBattleUnitsForSingleEnemy(BaseBattleUnit currentBattleUnit, List<BaseBattleUnit> enemyBattleUnitList, BaseBattleUnit enemyUnit)
        {
            List<BaseBattleUnit> currentEnemyUnitList = enemyBattleUnitList.Where(enemyBattleUnit => enemyBattleUnit.Id != enemyUnit.Id).ToList();
            currentEnemyUnitList.Add(currentBattleUnit);
            return currentEnemyUnitList;
        }

        private void RemoveDestroyedUnits(List<BaseBattleUnit> enemyBattleUnitList)
            => enemyBattleUnitList.RemoveAll(enemyUnit => enemyUnit.Health == 0);

        private List<BaseBattleUnit> GetLocalEnemyUnitList(string userId, BaseBattleUnit currentBattleUnit)
        {
            List<BaseBattleUnit> enemyBattleUnitList = GetBattleUnitEnemyList(userId);

            return !currentBattleUnit.UnitHasReachedDestination()
                ? enemyBattleUnitList.Where(enemyUnit => IsInSameStarSystemAsCurrentBattleUnit(enemyUnit, currentBattleUnit)).ToList()
                : GetEnemyBattleUnitsForArrivedUnit(currentBattleUnit, enemyBattleUnitList);
        }

        private List<BaseBattleUnit> GetEnemyBattleUnitsForArrivedUnit(BaseBattleUnit currentBattleUnit, List<BaseBattleUnit> enemyBattleUnitList)
            => currentBattleUnit.UnitLocation.Planet != null
                ? enemyBattleUnitList.Where(enemyUnit => IsInSameLocationWithPlanetAsCurrentBattleUnit(enemyUnit, currentBattleUnit)).ToList()
                : enemyBattleUnitList.Where(enemyUnit => IsInSameStarSystemAsCurrentBattleUnit(enemyUnit, currentBattleUnit)).ToList();

        private bool IsInSameLocationWithPlanetAsCurrentBattleUnit(BaseBattleUnit enemyUnit, BaseBattleUnit currentBattleUnit)
            => IsInSameStarSystemAsCurrentBattleUnit(enemyUnit, currentBattleUnit)
                && IsInSamePlanetAsCurrentBattleUnit(enemyUnit, currentBattleUnit);

        private bool IsInSameStarSystemAsCurrentBattleUnit(BaseBattleUnit enemyUnit, BaseBattleUnit currentBattleUnit)
            => enemyUnit.UnitLocation.StarSystem.Name == currentBattleUnit.UnitLocation.StarSystem.Name;

        private bool IsInSamePlanetAsCurrentBattleUnit(BaseBattleUnit enemyUnit, BaseBattleUnit currentBattleUnit)
            => enemyUnit.UnitLocation.Planet.Name == currentBattleUnit.UnitLocation.Planet.Name;

        private List<BaseBattleUnit> GetBattleUnitEnemyList(string userId)
        {
            List<User> enemyUserList = _userRepository.GetAllUsers().Where(user => user.Id != userId).ToList();
            List<GenericUnit> enemyUnitList = GetEnemyGenericUnitList(enemyUserList);
            return GetBattleUnitListFromEnemyUnitList(enemyUnitList);
        }

        private List<GenericUnit> GetEnemyGenericUnitList(List<User> enemyUserList)
        {
            var enemyUnitList = new List<GenericUnit>();
            enemyUserList.ForEach(enemyUser =>
            {
                List<GenericUnit> userBattleUnits = enemyUser.Units.Where(unit => IsBattleUnit(unit)).ToList();
                userBattleUnits.ForEach(enemyUnit => enemyUnitList.Add(enemyUnit));
            });
            return enemyUnitList;
        }

        private bool IsBattleUnit(GenericUnit unit)
            => unit.UnitType != UnitConstants.ScoutType && unit.UnitType != UnitConstants.BuilderType && unit.UnitType != UnitConstants.CargoType;

        private List<BaseBattleUnit> GetBattleUnitListFromEnemyUnitList(List<GenericUnit> enemyUnitList)
        {
            var enemyBattleUnits = new List<BaseBattleUnit>();
            enemyUnitList.ForEach(enemyUnit =>
            {
                FillEnemyBattleUnitList(enemyUnit, enemyBattleUnits);
            });
            return enemyBattleUnits;
        }

        private void FillEnemyBattleUnitList(GenericUnit enemyUnit, List<BaseBattleUnit> enemyBattleUnits)
        {
            BaseBattleUnit enemyBattleUnit = GetBattleUnitFromGenericUnit(enemyUnit);
            enemyBattleUnits.Add(enemyBattleUnit);
        }

        private BaseBattleUnit GetBattleUnitFromGenericUnit(GenericUnit battleUnit)
        {
            BaseBattleUnit enemyBattleUnit = GetBattleUnitInstanceConditionally(battleUnit);
            enemyBattleUnit.UnitDestination = battleUnit.UnitDestination;
            enemyBattleUnit.UnitLocation = battleUnit.UnitLocation;
            return enemyBattleUnit;
        }

        private BaseBattleUnit GetBattleUnitInstanceConditionally(GenericUnit enemyBattleUnit)
        {
            switch (enemyBattleUnit.UnitType)
            {
                case "fighter": return new FighterUnit(enemyBattleUnit.Id);  
                case "cruiser": return new CruiserUnit(enemyBattleUnit.Id);  
                case "bomber": return new BomberUnit(enemyBattleUnit.Id);
                default: throw new NotImplementedException();
            }
        }
    }
}
