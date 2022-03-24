using Shard.Shared.Core;
using System.Collections.Generic;

namespace Shard.WiemEtBrunelle.Web.Constants.Resources
{
    public class ResourcesConstants
    {

        public static readonly Dictionary<string, ResourceKind> resourcesName = new Dictionary<string, ResourceKind>()
        {
            { "carbon",  ResourceKind.Carbon},
            { "iron", ResourceKind.Iron  },
            { "gold", ResourceKind.Gold},
            { "aluminium", ResourceKind.Aluminium },
            { "titanium", ResourceKind.Titanium},
            { "water", ResourceKind.Water},
            { "oxygen", ResourceKind.Oxygen}
        };

        public static readonly Dictionary<ResourceKind, string> resourcesName2 = new Dictionary<ResourceKind, string>()
        {
            {  ResourceKind.Carbon, "carbon"},
            {  ResourceKind.Iron ,"iron" },
            {  ResourceKind.Gold, "gold"},
            {  ResourceKind.Aluminium,"aluminium" },
            {  ResourceKind.Titanium, "titanium"},
            {  ResourceKind.Water, "water"},
            {  ResourceKind.Oxygen,"oxygen"}
        };
    }
}
