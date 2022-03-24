using Shard.Shared.Core;
using System.Collections.Generic;

namespace Shard.WiemEtBrunelle.Web.Constants.Units
{
    public static class BuildingConstants
    {
        //Borne supérieure (comprise) de temps restant avant fin de construction d'un bâtiment en secondes
        //pour attendre ce temps restant avant de renvoyer une réponse à une requête GET sur /users/{userId}/buildings/{buildingId}
        public static readonly int RemainingSecondsLimitForRequestAwait = 2;


        public static readonly string SolidCategory = "solid";
        public static readonly string LiquidCategory = "liquid";
        public static readonly string GaseousCategory = "gaseous";

        public static readonly string BuildingExtractionType = "mine";
        public static readonly string BuildingConstructionType = "starport";

        
        public static readonly List<string> BuildingType = new List<string>() { "mine", "starport" };


        public static readonly List<string> resourcesCategory = new List<string>()
        {
            "solid",
            "liquid",
            "gaseous",
        };
        
        public static readonly List<ResourceKind> SolidResources = new List<ResourceKind>
        {
            ResourceKind.Aluminium,
            ResourceKind.Carbon,
            ResourceKind.Gold,
            ResourceKind.Iron,
            ResourceKind.Titanium
        };


        public static readonly List<ResourceKind> OrderOfRarityResources = new List<ResourceKind> 
        {
            ResourceKind.Titanium,
            ResourceKind.Gold,
            ResourceKind.Aluminium,
            ResourceKind.Iron,
            ResourceKind.Carbon,
        };

        public static readonly int DefaultResource = -1;

        //BUILDING OF UNIT BY STARTPORT
        public static readonly Dictionary<ResourceKind, int> CostScoutConstruction = new Dictionary<ResourceKind, int>()
        {
            {ResourceKind.Carbon, 5 },
            {ResourceKind.Iron, 5 }
        };
        public static readonly Dictionary<ResourceKind, int> CostBuilderConstruction = new Dictionary<ResourceKind, int>()
        {
            {ResourceKind.Carbon, 5 },
            {ResourceKind.Iron, 10 }
        };
        public static readonly Dictionary<ResourceKind, int> CostFighterConstruction = new Dictionary<ResourceKind, int>()
        {
            {ResourceKind.Iron, 20 },
            {ResourceKind.Aluminium, 10 }
        };
        public static readonly Dictionary<ResourceKind, int> CostBomberConstruction = new Dictionary<ResourceKind, int>()
        {
            {ResourceKind.Iron, 30 },
            {ResourceKind.Titanium, 10 }
        };
        public static readonly Dictionary<ResourceKind, int> CostCruiserConstruction = new Dictionary<ResourceKind, int>()
        {
            {ResourceKind.Iron, 60 },
            {ResourceKind.Gold, 20 }
        };
        public static readonly Dictionary<ResourceKind, int> CostCargoConstruction = new Dictionary<ResourceKind, int>()
        {
            {ResourceKind.Carbon, 10 },
            {ResourceKind.Iron, 10 },
            {ResourceKind.Gold, 5 }
        };


        public static readonly string MineType = "mine";
    }
}
