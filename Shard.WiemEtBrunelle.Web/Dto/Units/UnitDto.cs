using System.Collections.Generic;

namespace Shard.WiemEtBrunelle.Web.Dto.Units
{
    public class UnitDto
    {
        public UnitDto() { }

        public UnitDto(string id, string system, string planet, string type)
        {
            Id = id;
            System = system;
            Planet = planet;
            Type = type;
        }

        public string Id { get; set; }

        public string Type { get; set; }

        public string System { get; set; }

        public string Planet { get; set; }

        public string DestinationSystem { get; set; }

        public string DestinationPlanet { get; set; }

        public string EstimatedTimeOfArrival { get; set; }

        public int? Health { get; set; }

        public Dictionary <string, int> ResourcesQuantity { get; set; }

        public string DestinationShard { get; set; }
    }
}
