using Shard.Shared.Core;
using System.Collections.Generic;

namespace Shard.WiemEtBrunelle.Web.Converters
{
    public static class ResourcesConverter
    {
        public static Dictionary<string, int> GetLowerCasedResources(IReadOnlyDictionary<ResourceKind, int> modelResourcesQuantity)
        {
            var resourceQuantity = new Dictionary<string, int>();
            if (modelResourcesQuantity != null)
            {
                FillLowerCasedResources(resourceQuantity, modelResourcesQuantity);
            }
            return resourceQuantity;
        }

        private static void FillLowerCasedResources(Dictionary<string, int> resourceQuantity, 
            IReadOnlyDictionary<ResourceKind, int> modelResourcesQuantity)
        {
            foreach (KeyValuePair<ResourceKind, int> resourcePair in modelResourcesQuantity)
            {
                resourceQuantity.Add(resourcePair.Key.ToString().ToLower(), resourcePair.Value);
            }
        }
    }
}
