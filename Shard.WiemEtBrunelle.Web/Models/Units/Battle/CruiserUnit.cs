using Shard.WiemEtBrunelle.Web.Constants.Units;
using System.Collections.Generic;

namespace Shard.WiemEtBrunelle.Web.Models.Units.Battle
{
    public class CruiserUnit : BaseBattleUnit
    {
        public CruiserUnit(string id) : base(id) { }

        public override int Health { 
            get => UnitConstants.InitialCruiserHealth - DamageTaken < 0 ? 0 : UnitConstants.InitialCruiserHealth - DamageTaken;
            set => Health = value; 
        }

        public override string UnitType => UnitConstants.CruiserType;

        public override int AttackPower => UnitConstants.CruiserAttackPower;

        public override int CanonNumber => UnitConstants.CruiserCanonNumber;

        public override int FiringPeriodInSeconds => UnitConstants.CruiserFiringPeriodInSeconds;

        public override List<string> OrderedPrimaryTargets => UnitConstants.CruiserOrderedPrimaryTargets;
    }
}
