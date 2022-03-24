using Shard.WiemEtBrunelle.Web.Constants.Units;

namespace Shard.WiemEtBrunelle.Web.Models.Units
{
    public class ScoutUnit : GenericUnit
    {
        public ScoutUnit(string id) : base(id){}

        public override string UnitType => UnitConstants.ScoutType;

        public override bool CanScanPlanetResources => true;
    }
}
