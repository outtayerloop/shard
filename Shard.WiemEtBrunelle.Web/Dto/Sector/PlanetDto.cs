using Shard.Shared.Core;
using Shard.WiemEtBrunelle.Web.Models.Units;
using System.Collections.Generic;

namespace Shard.WiemEtBrunelle.Web.Dto
{
    public class PlanetDto
    {
        public PlanetDto(string name, int? size)
        {
            Name = name;
            Size = size;
        }

        public string Name { get; }

        public int? Size { get; }




    }
}
