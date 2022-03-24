using System.Collections.Generic;

namespace Shard.WiemEtBrunelle.Web.Constants.Units
{
    public static class UnitConstants
    {
        public static readonly int SecondsToReachNewSystem = 60;
        public static readonly int SecondsToEnterPlanet = 15;

        //Borne supérieure (comprise) de temps restant avant arrivée à destination d'un vaisseau en secondes
        //pour attendre ce temps restant avant de renvoyer une réponse à une requête GET sur /users/{userId}/units/{unitId}
        public static readonly int RemainingSecondsLimitForRequestAwait = 2;

        //Types de vaisseaux
        public static readonly string ScoutType = "scout";
        public static readonly string BuilderType = "builder";
        public static readonly string FighterType = "fighter";
        public static readonly string CargoType = "cargo";
        public static readonly string BomberType = "bomber";
        public static readonly string CruiserType = "cruiser";

        //Points de vie de depart des vaisseaux de combat
        public static readonly int InitialFighterHealth = 80;
        public static readonly int InitialCruiserHealth = 400;
        public static readonly int InitialBomberHealth = 50;
        public static readonly int InitialCargoHealth = 100;

        //Degats infliges par chaque type de vaisseau de combat
        public static readonly int FighterAttackPower = 10;
        public static readonly int CruiserAttackPower = 10;
        public static readonly int BomberAttackPower = 400;

        //Nombre de canons de chaque type de vaisseau de combat
        public static readonly int FighterCanonNumber = 1;
        public static readonly int CruiserCanonNumber = 4;
        public static readonly int BomberCanonNumber = 1;

        //Periodicite de tir de chaque type de vaisseau de combat (en secondes)
        public static readonly int FighterFiringPeriodInSeconds = 6;
        public static readonly int CruiserFiringPeriodInSeconds = 6;
        public static readonly int BomberFiringPeriodInSeconds = 60;

        //Classement des priorités des cibles de chaque type de vaisseau de combat (en secondes)
        public static readonly List<string> FighterOrderedPrimaryTargets = new List<string>() { BomberType, FighterType, CruiserType };
        public static readonly List<string> CruiserOrderedPrimaryTargets = new List<string>() { FighterType, CruiserType, BomberType };
        public static readonly List<string> BomberOrderedPrimaryTargets = new List<string>() { CruiserType, BomberType, FighterType };

        //Déflecteur du bombardier
        public static int BomberDeflectDivisor = 10;
    }
}
