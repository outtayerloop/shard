using System.Collections.Generic;

namespace Shard.WiemEtBrunelle.Web.Dto
{
    public class StarSystemDto
    {
        public StarSystemDto(IReadOnlyList<PlanetDto> planets, string name)
        {
            Planets = planets;
            Name = name;
        }

        public string Name { get; }

        public IReadOnlyList<PlanetDto> Planets { get; }
    }
}
