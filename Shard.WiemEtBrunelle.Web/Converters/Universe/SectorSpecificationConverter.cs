using Shard.Shared.Core;
using Shard.WiemEtBrunelle.Web.Models.Universe;
using System.Collections.Generic;

namespace Shard.WiemEtBrunelle.Web.Converters.Universe
{
    public static class SectorSpecificationConverter
    {
        public static Sector ConvertToSector(SectorSpecification sectorSpecification)
        {
            if (sectorSpecification == null) 
                return null;

            IReadOnlyList<StarSystem> systems = ConvertToStarSystems(sectorSpecification.Systems);
            var sector = new Sector(systems);
            return sector;
        }

        private static IReadOnlyList<StarSystem> ConvertToStarSystems(IReadOnlyList<SystemSpecification> systemSpecificationList)
        {
            var systems = new List<StarSystem>();
            foreach (SystemSpecification systemSpecification in systemSpecificationList)
            {
                List<Planet> planets = ConvertToPlanets(systemSpecification.Planets);
                var system = new StarSystem(planets, systemSpecification.Name);
                systems.Add(system);
            }
            return systems;
        }

        private static List<Planet> ConvertToPlanets(IReadOnlyList<PlanetSpecification> planetSpecificationList)
        {
            var planets = new List<Planet>();
            foreach (PlanetSpecification planetSpecification in planetSpecificationList)
            {
                var planet = new Planet(planetSpecification.Name, planetSpecification.Size, (Dictionary<ResourceKind, int>)planetSpecification.ResourceQuantity);
                planets.Add(planet);
            }
            return planets;
        }
    }
}
