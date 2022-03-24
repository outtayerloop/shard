using Shard.WiemEtBrunelle.Web.Models.Units;
using System.Collections.Generic;

namespace Shard.WiemEtBrunelle.Web.Dto.Units
{
    public class BuildingDto
    {
        public BuildingDto() { }

        public BuildingDto(string id, string type)
        {
            Id = id;
            Type = type;
        }

        public string Id { get; set; }

        public string Type { get; set; }

        public string ResourceCategory { get; set; }

        public string BuilderId { get; set; }

        public string System { get; set; }

        public string Planet { get; set; }

        public bool IsBuilt { get; set; }

        public string EstimatedBuildTime { get ;set; }
        public List<UnitDto> UnitsQueue { get; set; }



    }
}
