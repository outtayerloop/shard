using Shard.WiemEtBrunelle.Web.Constants.Units;

namespace Shard.WiemEtBrunelle.Web.Models.Units
{
    public class BuilderUnit : GenericUnit
    {
        public BuilderUnit(string id) : base(id){}

        public override string UnitType => UnitConstants.BuilderType;

        public override bool CanScanPlanetResources => false;
    }
}
