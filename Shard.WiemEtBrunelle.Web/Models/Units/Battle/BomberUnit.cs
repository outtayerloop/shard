using Shard.WiemEtBrunelle.Web.Constants.Units;
using System.Collections.Generic;

namespace Shard.WiemEtBrunelle.Web.Models.Units.Battle
{
    public class BomberUnit : BaseBattleUnit
    {

        public BomberUnit(string id) : base(id) { }

        public override int Health { 
            get => UnitConstants.InitialBomberHealth - DamageTaken < 0 ? 0 : UnitConstants.InitialBomberHealth - DamageTaken; 
            set => Health = value; 
        }

        public override string UnitType => UnitConstants.BomberType;

        public override int AttackPower => UnitConstants.BomberAttackPower;

        public override int CanonNumber => UnitConstants.BomberCanonNumber;

        public override int FiringPeriodInSeconds => UnitConstants.BomberFiringPeriodInSeconds;

        public override List<string> OrderedPrimaryTargets => UnitConstants.BomberOrderedPrimaryTargets;
    }
}
