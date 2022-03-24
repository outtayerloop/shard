namespace Shard.WiemEtBrunelle.Web.Constants.Requests
{
    public static class RequestConstants
    {
        //Si la requête est valide
        public static readonly string Ok = "ok";

        //Requêtes nécessitant un body
        public static readonly string MissingBody = "Cette requête nécessite des données dans un corps.";
        public static readonly string ConflictedIds = "L'ID d'entité fourni dans le corps de la requête doit correspondre au paramètre associé dans l'URL.";

        //Spécificités des vaisseaux
        public static readonly string MissingDestinationSystem = "Le vaisseau doit avoir un système de destination.";
        public static readonly string ConflictedBuilderAndBuildingPlanets = "Le constructeur doit se situer sur la même planète que celle de la construction.";
        public static readonly string MissingBuilderPlanet = "Le constructeur doit se situer sur une planète pour construire.";

        //Spécificité des utilisateurs
        public static readonly string BadUserId = "L'ID de l'utilisateur doit être une suite alphanumérique pouvant contenir des tirets.";

        public static readonly string BadBuildingData = "Les données de la mine ne sont pas conformes";
        public static readonly string BadBuilderId = "L'ID du vaisseau est différent de celui du constructeur de la mine";
        
        //Relatif à l'authentification
        public static bool AdminIsAuthenticated = false;
        public static bool NewUserIsAuthenticated = false;
        public static readonly string AuthorizationHeaderName = "Authorization";
        public static readonly string MissingHeaderAuthorization = "Header d'autorisation manquant";

        //Connexion Shard
        public static readonly string UserRemotePseudo = "remote.user";
        public static readonly int BadGatewayHttpStatus = 502;
        public static readonly string BadDestinationShard = "Shard de destination spécifié incorrect";

    }
}
