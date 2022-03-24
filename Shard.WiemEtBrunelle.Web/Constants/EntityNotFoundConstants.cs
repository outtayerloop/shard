using System.Collections.Generic;

namespace Shard.WiemEtBrunelle.Web.Constants
{
    public static class EntityNotFoundConstants
    {
        public static readonly Dictionary<string, string> EntityNamePairs = new Dictionary<string, string>()
        {
            { "StarSystemDto", "Systeme" },
            { "PlanetDto", "Planete" },
            { "UserDto", "Utilisateur" },
            { "SpaceUnitDto", "Vaisseau" },
            { "BuildingDto", "Mine" },
            {"UnitDto", "Unit" }
        };

        public static readonly string EntityNotFoundMessage = "NotFound";
    }
}
