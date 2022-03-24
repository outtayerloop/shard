using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shard.WiemEtBrunelle.Web.Constants;
using Shard.WiemEtBrunelle.Web.Constants.Requests;
using Shard.WiemEtBrunelle.Web.Constants.Units;
using Shard.WiemEtBrunelle.Web.Dto;
using Shard.WiemEtBrunelle.Web.Dto.Units;
using Shard.WiemEtBrunelle.Web.Dto.Users;
using Shard.WiemEtBrunelle.Web.Models;
using Shard.WiemEtBrunelle.Web.Models.Problem;
using Shard.WiemEtBrunelle.Web.Services.Problem;
using Shard.WiemEtBrunelle.Web.Services.RequestValidators;
using Shard.WiemEtBrunelle.Web.Services.Users;

/*********************************
 * 
 * Nom: UsersController
 * 
 * Rôle: Contrôleur de la route /Users
 * 
 * Date: 19/09/2020
 * 
 ********************************/

namespace Shard.WiemEtBrunelle.Web.Controllers
{
    [Route("users")]
    public class UsersController : ShardBaseController
    {
        public UsersController(IUserService userService, IProblemDescriptionService problemDescriptionService,
            IRequestBodyValidationService requestValidationService)
            : base(userService, problemDescriptionService, requestValidationService) { }

        [AllowAnonymous]
        [HttpGet("{userId}/units")]
        public async Task<ActionResult<List<UnitDto>>> GetAllUnitsFromUser(string userId)
        {
            List<UnitDto> units = await _userService.GetAllUnitsFromUser(userId);

            UserDto userFound =  _userService.GetUserById(userId);
            if (userFound.Pseudo == RequestConstants.UserRemotePseudo)
                return Ok(units);

            return RetrievalResponse<UserDto, UnitDto>(units, RetrievalContext.Several);
        }

        [AllowAnonymous]
        [HttpGet("{userId}/units/{unitId}")]
        public async Task<ActionResult<UnitDto>> GetSingleUnitFromUser(string userId, string unitId)
        {
            UnitDto unit = await _userService.GetSingleUnitFromUser(userId, unitId);
            if (unit.Type == UnitConstants.CargoType)
                return Ok(unit);
            return SingleEntityTiedToUserAndSystemResponse(unit, unit?.Id, unit?.System, unit?.Planet);
        }
        
        [HttpPut("{userId}/units/{unitId}")]
        public async Task<ActionResult<UnitDto>>UpdateSingleUnitFromUser(string userId, string unitId, [FromBody] UnitDto unitBodyData)
        {
            if (RequestConstants.AdminIsAuthenticated) 
                return _userService.PutUnitAsAdministrator(userId, unitBodyData);

            if (RequestConstants.NewUserIsAuthenticated)
                return Ok(_userService.ReceivingJumpingCargo(userId, unitBodyData));
            
            string requestValidityMessage = _requestValidationService.GetUnitUpdateRequestValidity(unitId, unitBodyData);

            if (requestValidityMessage != RequestConstants.Ok && unitBodyData.ResourcesQuantity==null)
                return GetUnitUpdateInvalidRequestResponse(requestValidityMessage);

            UnitDto updatedUnit;
            string remoteRedirectionUri;

            if (unitBodyData.DestinationShard != null)
            {
                (updatedUnit, remoteRedirectionUri) = await _userService.TeleportUnitToAnotherShard(userId, unitBodyData);

                if(updatedUnit != null && updatedUnit.Id != null && updatedUnit.System != null)
                {
                    if(updatedUnit.DestinationShard == null)
                    {
                        return Problem(null, null, RequestConstants.BadGatewayHttpStatus, null, null);
                    }
                    else
                    {
                        return RedirectPermanentPreserveMethod(remoteRedirectionUri);
                    }
                }

            }
            else
            {
                updatedUnit = _userService.UpdateSingleUnitFromUser(userId, unitBodyData);

                if (updatedUnit == null || updatedUnit.Type == UnitConstants.CargoType)
                    return HandlerUpdateUnitResult(updatedUnit);
            }

            return SingleEntityTiedToUserAndSystemResponse(updatedUnit, updatedUnit.Id, updatedUnit.System, updatedUnit.Planet);
        }

        private ActionResult HandlerUpdateUnitResult(UnitDto updateUnit)
        {
            if (updateUnit == null)
                return BadRequest();
            return Ok();
        }
        /// <summary>
        /// Crée ou met à jour un utilisateur selon qu'il existait déjà ou non
        /// </summary>
        /// <param name="userId">Identifiant de l'utilisateur à créer ou mettre à jour</param>
        /// <param name="userBodyData">Données de création ou de mise à jour</param>
        /// <returns></returns>

        [HttpPut("{userId}")]
        public ActionResult<UserDto> CreateOrUpdateUser(string userId, UserDto userBodyData)
        {

            string requestValidityMessage = _requestValidationService.GetUserUpdateOrCreateRequestValidity(userId, userBodyData);

            if (requestValidityMessage != RequestConstants.Ok) 
                return GetBadRequestResponse(requestValidityMessage);

            UserDto updatedOrCreatedUser = _userService.CreateOrUpdateUser(userBodyData);
            return UserResponse(updatedOrCreatedUser);
        }

        /// <summary>
        /// Récupère un utilisateur par son idenfifiant
        /// </summary>
        /// <param name="userId">Identifiant de l'utilisateur recherché</param>
        /// <returns></returns>
        [HttpGet("{userId}")]
        public ActionResult<UserDto> GetUserById(string userId)
        {
            UserDto userFound = _userService.GetUserById(userId);
            return UserResponse(userFound);
        }

        /// <summary>
        /// Récupère des informations sur la localisation d'un vaisseau pour un utilisateur donné
        /// </summary>
        /// <param name="userId">Identifiant de l'utilisateur</param>
        /// <param name="unitId">Identifiant du vaisseau</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("{userId}/Units/{unitId}/location")]
        public async Task<ActionResult<GenericUnitLocationDto>> GetSingleUnitDetailsForUser(string userId, string unitId)
        {
            GenericUnitLocationDto unitDetails = await _userService.GetSingleUnitDetailsForUser(userId, unitId);
            var unitDetailsList = new List<GenericUnitLocationDto>() { unitDetails };
            return RetrievalResponse(unitDetailsList, RetrievalContext.Single);
        }

        [AllowAnonymous]
        [HttpPost("{userId}/buildings")]
        public async Task<ActionResult<BuildingDto>> CreateBuilding(string userId, [FromBody] BuildingDto buildingData)
        {
            UnitDto builderUnit = await _userService.GetUnitOfSpecificType(userId, UnitConstants.BuilderType);

            if (builderUnit == null) 
                return RetrievalResponse<UserDto, UnitDto>(null, RetrievalContext.Single);

            if (builderUnit.Id == null) 
                return RetrievalResponse<UnitDto>(null, RetrievalContext.Single);

            string requestValidityMessage = _requestValidationService.GetBuildingRequestValidity(buildingData, builderUnit);

            if (requestValidityMessage != RequestConstants.Ok) 
                return GetBadRequestResponse(requestValidityMessage);

            BuildingDto newBuilding = _userService.OrderBuildingCreation(buildingData, builderUnit);
            
            return RetrievalResponse(new List<BuildingDto>() { newBuilding }, RetrievalContext.Single);
        }

        /// <summary>
        /// Récupère l'ensemble des bâtiments d'un utilisateur
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet("{userId}/buildings")]
        public ActionResult<BuildingDto> GetAllBuildingsFromUser(string userId)
        {
            List<BuildingDto> userBuildings = _userService.GetAllBuildingsFromUser(userId);

            if (userBuildings == null) 
                return RetrievalResponse<UserDto, BuildingDto>(null, RetrievalContext.Several);

            return RetrievalResponse(userBuildings, RetrievalContext.Several);
        }

        /// <summary>
        /// Récupère un bâtiment spécifique d'un utilisateur
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="buildingId"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("{userId}/buildings/{buildingId}")]
        public async Task<ActionResult<BuildingDto>> GetSingleBuildingFromUser(string userId, string buildingId)
        {
            BuildingDto building = await _userService.GetSingleBuildingFromUser(userId, buildingId);
            return SingleEntityTiedToUserAndSystemResponse(building, building?.Id, building?.System, building?.Planet);
        }

        private ActionResult<UnitDto> GetUnitUpdateInvalidRequestResponse(string requestValidityMessage)
        {
            if (!RequestConstants.AdminIsAuthenticated)
                return Unauthorized();
            return GetBadRequestResponse(requestValidityMessage);
        }

        private ActionResult GetBadRequestResponse(string badRequestReason)
        {
            ProblemDescription badRequestProblemDescription = _problemDescriptionService
                .GetBadRequestProblemDescription(HttpContext, badRequestReason);

            return BadRequest(badRequestProblemDescription);
        }

        [AllowAnonymous]
        [HttpPost("{userId}/buildings/{starportId}/queue")]
        public async Task<ActionResult> QueuingUnit(string userId, string starportId, [FromBody] UnitDto newUnit)
        {
  
            if (!UserFound(userId))
                return NotFound();

            if (!GetValidityRequest(starportId))
                return NotFound();

            BuildingDto buildingFound = await _userService.GetSingleBuildingFromUser(userId, starportId);

            if (buildingFound==null || buildingFound.Type == BuildingConstants.BuildingExtractionType)
                return HandlerBuildingFound(buildingFound);
            
            
            UnitDto unitCreatedByStarport = await _userService.StartPortCreatesUnitAsync(userId, starportId, newUnit);

            if (unitCreatedByStarport == null) 
                return BadRequest(); 

            return Ok(unitCreatedByStarport);
        }
       
        
        private bool UserFound(string userId)
        {
            bool found = true;
            UserDto userFound = _userService.GetUserById(userId);

            if (userFound == null)
                return false;

            return found;

        }

        private bool GetValidityRequest(string starportId)
        {
            bool valid = true;

            string requestValidityMessage = _requestValidationService.GetBuildingRequestValidity(starportId);

            if (requestValidityMessage != RequestConstants.Ok)
                return false;

            return valid;
        }
        private ActionResult HandlerBuildingFound(BuildingDto buildingFound)
        {

            if (buildingFound == null)
                return NotFound();

            return BadRequest();
        }
        /// <summary>
        /// Retourne une réponse HTTP contenant, pour un utilisateur donné, ses données recherchées, créées ou mises
        /// à jour en cas de succès.
        /// </summary>
        /// <param name="user">Vaisseau recherché, créé ou ou mis à jour</param>
        /// <returns></returns>
        private ActionResult<UserDto> UserResponse(UserDto user)
        {
            var userList = new List<UserDto>() { user };
            return RetrievalResponse(userList, RetrievalContext.Single);
        }

        /// <summary>
        /// Retourne une réponse HTTP contenant, pour un utilisateur donné, l'entité recherchée en cas de succès.
        /// </summary>
        /// <param name="entity">Entité recherchée</param>
        /// <param name="entityId">ID de l'entité recherchée</param>
        /// <param name="entitySystem">Système de l'entité recherchée</param>
        /// <param name="entityPlanet">planète de l'entité recherchée</param>
        /// <returns></returns>
        private ActionResult<T> SingleEntityTiedToUserAndSystemResponse<T>(T entity, string entityId, string entitySystem,
            string entityPlanet)
        {
            List<T> entityList = GetSingleEntityList(entity, entityId);

            if (entitySystem == null) 
                return RetrievalResponse<StarSystemDto, T>(null, RetrievalContext.Single);

            if (entityPlanet == EntityNotFoundConstants.EntityNotFoundMessage)
                return RetrievalResponse<PlanetDto>(null, RetrievalContext.Single);

            return RetrievalResponse<UserDto, T>(entityList, RetrievalContext.Single);
        }

        /// <summary>
        /// Retourne null si l'entité est null, une nouvelle liste contenant une entité null si l'ID de l'entité est null,
        /// sinon retourne une liste contenant l'entité
        /// </summary>
        /// <param name="entity">Entité récupérée ou mise à jour</param>
        /// <param name="entityId">ID de l'entité</param>
        /// <returns></returns>
        private List<T> GetSingleEntityList<T>(T entity, string entityId)
        {
            if (entity == null) 
                return null;

            return entityId == null 
                ? new List<T>() 
                : new List<T>() { entity };
        }
    }

}
