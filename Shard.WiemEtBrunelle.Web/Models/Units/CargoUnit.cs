using Shard.WiemEtBrunelle.Web.Constants.Units;

namespace Shard.WiemEtBrunelle.Web.Models.Units
{
    public class CargoUnit : GenericUnit
    {
        public CargoUnit(string id) : base(id) { }

        public override string UnitType => UnitConstants.CargoType;

        public override int Health { get; set; } = UnitConstants.InitialCargoHealth;

        public override bool CanScanPlanetResources => false;

        public string System { get; set; }

        public string Planet { get; set; }
    }
}
