using Shard.WiemEtBrunelle.Web.Dto.Users;
using Shard.WiemEtBrunelle.Web.Dto.Units;
using System.Text.RegularExpressions;
using Shard.WiemEtBrunelle.Web.Constants.Requests;
using Microsoft.Extensions.Configuration;

namespace Shard.WiemEtBrunelle.Web.Services.RequestValidators
{
    public class RequestBodyValidationService : IRequestBodyValidationService
    {

        private IConfiguration _configuration;

        public RequestBodyValidationService(IConfiguration configuration)
            => _configuration = configuration;

        public string GetUnitUpdateRequestValidity(string unitId, UnitDto unitBodyData)
        {
            if (unitBodyData == null)
                return RequestConstants.MissingBody;

            if (unitId != unitBodyData.Id)
                return RequestConstants.ConflictedIds;

            return EnsureUnitDestinationDataValidity(unitBodyData);
        }

        public string GetUserUpdateOrCreateRequestValidity(string userId, UserDto userBodyData)
        {
            if (userBodyData == null)
                return RequestConstants.MissingBody;

            return EnsureUserDataValidity(userId, userBodyData);
        }

        public string GetBuildingRequestValidity(string buildingId)
        {
            if (buildingId == null)
                return RequestConstants.MissingBody;

            return RequestConstants.Ok;
        }

        public string GetBuildingRequestValidity(BuildingDto buildData, UnitDto builder)
        {
            if (HasInvalidBuildingData(buildData))
                return RequestConstants.BadBuildingData;

            return CheckBuilderDataValidity(buildData, builder);
        }

        private string CheckBuilderDataValidity(BuildingDto buildData, UnitDto builder)
        {
            if (buildData.BuilderId != builder.Id)
                return RequestConstants.BadBuilderId;

            else if (BuilderIsNotOnPlanet(builder))
                return RequestConstants.MissingBuilderPlanet;

            else if (BuildingPlanetIsDifferentFromBuilderPlanet(buildData, builder))
                return RequestConstants.ConflictedBuilderAndBuildingPlanets;

            return RequestConstants.Ok;
        }

        private bool BuildingPlanetIsDifferentFromBuilderPlanet(BuildingDto buildData, UnitDto builder)
            => buildData.Planet != null && builder.Planet != buildData.Planet;

        private bool BuilderIsNotOnPlanet(UnitDto builder)
            => string.IsNullOrEmpty(builder.Planet) || string.IsNullOrWhiteSpace(builder.Planet);

        private bool HasInvalidBuildingData(BuildingDto buildData)
            => buildData == null || BuildingDoesNotHaveBuilderAssigned(buildData) || buildData.ResourceCategory == null;

        private bool BuildingDoesNotHaveBuilderAssigned(BuildingDto buildData)
            => string.IsNullOrEmpty(buildData.BuilderId) || string.IsNullOrWhiteSpace(buildData.BuilderId);

        /// <summary>
        /// Retourne un message d'erreur si le système de destination du vaisseau est absent ou vide, sinon retourne "ok".
        /// </summary>
        /// <param name="unitBodyData">Body de la requête de modification de vaisseau</param>
        /// <returns></returns>
        private string EnsureUnitDestinationDataValidity(UnitDto unitBodyData)
        {
            if(unitBodyData.DestinationShard == null)
            {
                if (IsEmptyDestinationSystem(unitBodyData.DestinationSystem))
                    return RequestConstants.MissingDestinationSystem;
            }
            else
            {
                if (HasBadDestinationShard(unitBodyData))
                    return RequestConstants.BadDestinationShard;
            }

            return RequestConstants.Ok;
        }

        private bool HasBadDestinationShard(UnitDto unitBodyData)
            => _configuration.GetValue<string>($"Wormholes:{unitBodyData.DestinationShard}:baseUri") == null;

        private bool IsEmptyDestinationSystem(string destinationSystem)
            => string.IsNullOrEmpty(destinationSystem);

        /// <summary>
        /// Retourne un message d'erreur si l'ID d'utilisateur du body de la requête ne correspond pas à celui fourni en paramètre
        /// de la requête ou si l'ID de l'utilisateur ne contient pas au minimum 2 caractères, sinon retourne "ok".
        /// </summary>
        /// <param name="userId">ID d'utilisateur passé en paramètre de la requête de création/modification d'utilisateur</param>
        /// <param name="userBodyData">Body de la requête de création/modification d'utilisateur</param>
        /// <returns></returns>
        private string EnsureUserDataValidity(string userId, UserDto userBodyData)
        {
            if (userId != userBodyData.Id)
                return RequestConstants.ConflictedIds;

            var idValidationRegex = new Regex(@"^[\w-_]+$");

            if (!idValidationRegex.IsMatch(userId))
                return RequestConstants.BadUserId;

            return RequestConstants.Ok;
        }
       
    }
}
