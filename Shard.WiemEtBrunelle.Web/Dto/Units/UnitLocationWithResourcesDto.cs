using System.Collections.Generic;

namespace Shard.WiemEtBrunelle.Web.Dto.Units
{
    public class UnitLocationWithResourcesDto : GenericUnitLocationDto
    {
        public UnitLocationWithResourcesDto(IReadOnlyDictionary<string, int> resourceQuantity)
            => ResourcesQuantity = resourceQuantity;

        public IReadOnlyDictionary<string, int> ResourcesQuantity { get; }

    }
}
