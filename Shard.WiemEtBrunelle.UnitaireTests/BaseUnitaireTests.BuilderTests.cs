using Shard.WiemEtBrunelle.Web.Models.Units;
using Xunit;

namespace Shard.WiemEtBrunelle.UnitaireTests
{
    public partial class BaseUnitaireTests
    {
        [Fact]
        public void GetUnitType_FromBuilderUnit_ReturnsBuilderType()
        {
            var builderUnit = new BuilderUnit("1");

            string builderUnitType = builderUnit.UnitType;

            Assert.Equal("builder", builderUnitType);
        }

        [Fact]
        public void CanScanPlanetResources_FromBuilderUnit_ReturnsFalse()
        {
            GenericUnit builderUnit = new BuilderUnit("1");

            bool actualScanAbility = builderUnit.CanScanPlanetResources;

            Assert.False(actualScanAbility);
        }
    }
}
