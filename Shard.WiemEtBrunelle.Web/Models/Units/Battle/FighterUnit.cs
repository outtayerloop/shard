using Shard.WiemEtBrunelle.Web.Constants.Units;
using System.Collections.Generic;

namespace Shard.WiemEtBrunelle.Web.Models.Units.Battle
{
    public class FighterUnit : BaseBattleUnit
    {

        public FighterUnit(string id) : base(id) { }

        public override int Health
        {
            get => UnitConstants.InitialFighterHealth - DamageTaken < 0 ? 0 : UnitConstants.InitialFighterHealth - DamageTaken;
            set => Health = value;
        }

        public override string UnitType => UnitConstants.FighterType;

        public override int AttackPower => UnitConstants.FighterAttackPower;

        public override int CanonNumber => UnitConstants.FighterCanonNumber;

        public override int FiringPeriodInSeconds => UnitConstants.FighterFiringPeriodInSeconds;

        public override List<string> OrderedPrimaryTargets => UnitConstants.FighterOrderedPrimaryTargets;
    }
}
