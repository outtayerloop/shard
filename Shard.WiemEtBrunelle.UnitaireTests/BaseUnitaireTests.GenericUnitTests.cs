using Shard.Shared.Core;
using Shard.WiemEtBrunelle.Web.Models.Units;
using Shard.WiemEtBrunelle.Web.Models.Units.GeographicData;
using Shard.WiemEtBrunelle.Web.Models.Universe;
using System;
using System.Collections.Generic;
using Xunit;

namespace Shard.WiemEtBrunelle.UnitaireTests
{
    public partial class BaseUnitaireTests
    {
        [Fact]
        public void GetUnitType_FromGenericUnit_ReturnsNull()
        {
            var genericUnit = new GenericUnit();

            string actualUnitType = genericUnit.UnitType;

            Assert.Null(actualUnitType);
        }

        [Fact]
        public void CanScanPlanetResources_FromGenericUnit_ReturnsFalse()
        {
            var genericUnit = new GenericUnit();

            bool actualScanAbility = genericUnit.CanScanPlanetResources;

            Assert.False(actualScanAbility);
        }

        [Theory]
        [InlineData("scout")]
        [InlineData("builder")]
        public void ReachDestination_WhenNotReachedYetAndContainsPlanet_CopiesDestinationSystemAndPlanetInLocation(string unitType)
        {
            GenericUnit genericUnit = CreateUnitWithEmptyLocation(unitType, "1");
            Planet stubDestinationPlanet = CreatePlanet("mars", 10, null);
            StarSystem stubDestinationSystem = CreateStarSystem("omega", new List<Planet> { stubDestinationPlanet });
            UnitDestination stubDestination = CreateUnitDestination(stubDestinationSystem, stubDestinationPlanet, DateTime.Now, new TimeSpan(0, 0, 0));
            genericUnit.UnitDestination = stubDestination;

            genericUnit.ReachDestination();

            Assert.Equal(genericUnit.UnitLocation.StarSystem, stubDestinationSystem);
            Assert.Equal(genericUnit.UnitLocation.Planet, stubDestinationPlanet);
        }

        [Theory]
        [InlineData("scout")]
        [InlineData("builder")]
        public void ReachDestination_WhenNotReachedYetAndDoesNotContainPlanet_CopiesDestinationSystemAndPlanetInLocation(string unitType)
        {
            GenericUnit genericUnit = CreateUnitWithEmptyLocation(unitType, "1");
            Planet stubfillerPlanet = CreatePlanet("mars", 10, null);
            StarSystem stubDestinationSystem = CreateStarSystem("omega", new List<Planet> { stubfillerPlanet });
            UnitDestination stubDestination = CreateUnitDestination(stubDestinationSystem, null, DateTime.Now, new TimeSpan(0, 0, 0));
            genericUnit.UnitDestination = stubDestination;

            genericUnit.ReachDestination();

            Assert.Equal(genericUnit.UnitLocation.StarSystem, stubDestinationSystem);
            Assert.Null(genericUnit.UnitLocation.Planet);
        }

        [Theory]
        [InlineData("scout")]
        [InlineData("builder")]
        public void ReachDestination_WhenAlreadyThereAndContainsPlanet_CopiesDestinationSystemAndPlanetInLocation(string unitType)
        {
            Planet stubDestinationPlanet = CreatePlanet("mars", 10, null);
            StarSystem stubDestinationSystem = CreateStarSystem("omega", new List<Planet> { stubDestinationPlanet });
            GenericUnit genericUnit = CreateUnitWithFilledLocation(unitType, "1", stubDestinationSystem, stubDestinationPlanet);
            UnitDestination stubDestination = CreateUnitDestination(stubDestinationSystem, stubDestinationPlanet, DateTime.Now, new TimeSpan(0, 0, 0));
            genericUnit.UnitDestination = stubDestination;

            genericUnit.ReachDestination();

            Assert.Equal(genericUnit.UnitLocation.StarSystem, stubDestinationSystem);
            Assert.Equal(genericUnit.UnitLocation.Planet, stubDestinationPlanet);
        }

        [Theory]
        [InlineData("scout")]
        [InlineData("builder")]
        public void ReachDestination_WhenAlreadyThereAndDoesNotContainPlanet_CopiesDestinationSystemAndPlanetInLocation(string unitType)
        {
            Planet stubfillerPlanet = CreatePlanet("mars", 10, null);
            StarSystem stubDestinationSystem = CreateStarSystem("omega", new List<Planet> { stubfillerPlanet });
            GenericUnit genericUnit = CreateUnitWithFilledLocation(unitType, "1", stubDestinationSystem, null);
            UnitDestination stubDestination = CreateUnitDestination(stubDestinationSystem, null, DateTime.Now, new TimeSpan(0, 0, 0));
            genericUnit.UnitDestination = stubDestination;

            genericUnit.ReachDestination();

            Assert.Equal(genericUnit.UnitLocation.StarSystem, stubDestinationSystem);
            Assert.Null(genericUnit.UnitLocation.Planet);
        }

        private GenericUnit CreateUnitWithEmptyLocation(string unitType, string stubUnitId)
        {
            GenericUnit genericUnit = CreateUnitOfGivenType(unitType, stubUnitId);
            Assert.NotNull(genericUnit);

            BaseUnitLocation stubLocation = CreateEmptyUnitLocationFromUnitType(unitType);
            genericUnit.UnitLocation = stubLocation;
            return genericUnit;
        }

        private GenericUnit CreateUnitWithFilledLocation(string unitType, string stubUnitId, StarSystem stubSystem, Planet stubPlanet)
        {
            GenericUnit genericUnit = CreateUnitWithEmptyLocation(unitType, stubUnitId);
            genericUnit.UnitLocation.StarSystem = stubSystem;
            genericUnit.UnitLocation.Planet = stubPlanet;
            return genericUnit;
        }

        private GenericUnit CreateUnitOfGivenType(string unitType, string stubUnitId)
        {
            if (unitType == "scout") return new ScoutUnit(stubUnitId);
            else if (unitType == "builder") return new BuilderUnit(stubUnitId);
            else return null;
        }

        private UnitDestination CreateUnitDestination(StarSystem destinationSystem, Planet destinationPlanet,
            DateTime dateOfArrival, TimeSpan estimatedTimeOfArrival)
        {
            var unitDestination = new UnitDestination()
            {
                StarSystem = destinationSystem,
                Planet = destinationPlanet,
                DateOfArrival = dateOfArrival,
                EstimatedTimeOfArrival = estimatedTimeOfArrival
            };

            return unitDestination;
        }

        private BaseUnitLocation CreateEmptyUnitLocationFromUnitType(string unitType)
        {
            if (unitType == "scout") return new UnitLocationWithResources();
            else if (unitType == "builder") return new ResourcelessUnitLocation();
            else return null;
        }

        private StarSystem CreateStarSystem(string systemName, IReadOnlyList<Planet> planetList)
            => new StarSystem(planetList, systemName);

        private Planet CreatePlanet(string planetName, int? planetSize, IReadOnlyDictionary<ResourceKind, int> planetResources)
            => new Planet(planetName, planetSize, planetResources);
    }
}
