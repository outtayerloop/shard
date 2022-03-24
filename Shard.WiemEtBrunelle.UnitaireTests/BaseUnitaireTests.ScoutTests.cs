using Shard.WiemEtBrunelle.Web.Models.Units;
using Xunit;

namespace Shard.WiemEtBrunelle.UnitaireTests
{
    partial class BaseUnitaireTests
    {
        [Fact]
        public void GetUnitType_FromScoutUnit_ReturnsScoutType()
        {
            var scoutUnit = new ScoutUnit("1");

            string actualUnitType = scoutUnit.UnitType;

            Assert.Equal("scout", actualUnitType);
        }

        [Fact]
        public void CanScanPlanetResources_FromScoutUnit_ReturnsTrue()
        {
            GenericUnit scoutUnit = new ScoutUnit("1");

            bool actualScanAbility = scoutUnit.CanScanPlanetResources;

            Assert.True(actualScanAbility);
        }
    }
}
