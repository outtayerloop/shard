using System.Collections.Generic;

namespace Shard.WiemEtBrunelle.Web.Models.Universe
{
    public class Sector
    {
        public Sector(IReadOnlyList<StarSystem> systems)
            => Systems = systems;

        public IReadOnlyList<StarSystem> Systems { get; }
    }
}
