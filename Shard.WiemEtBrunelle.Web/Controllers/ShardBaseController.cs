using Microsoft.AspNetCore.Mvc;
using Shard.WiemEtBrunelle.Web.Constants.Requests;
using Shard.WiemEtBrunelle.Web.Models;
using Shard.WiemEtBrunelle.Web.Models.Problem;
using Shard.WiemEtBrunelle.Web.Services;
using Shard.WiemEtBrunelle.Web.Services.Problem;
using Shard.WiemEtBrunelle.Web.Services.RequestValidators;
using Shard.WiemEtBrunelle.Web.Services.Users;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Shard.WiemEtBrunelle.Web.Controllers
{
    [ApiController]
    public abstract class ShardBaseController : ControllerBase
    {
        protected readonly ISectorService _sectorService;
        protected readonly IUserService _userService;
        protected readonly IProblemDescriptionService _problemDescriptionService;
        protected readonly IRequestBodyValidationService _requestValidationService;
        

        public ShardBaseController(ISectorService sectorService, IProblemDescriptionService problemDescriptionService)
        {
            _sectorService = sectorService;
            _problemDescriptionService = problemDescriptionService;
        }


        public ShardBaseController(IUserService userService, IProblemDescriptionService problemDescriptionService,
            IRequestBodyValidationService requestValidationService)
        {
            _userService = userService;
            _problemDescriptionService = problemDescriptionService;
            _requestValidationService = requestValidationService;
        }

        /// <summary>
        /// Retourne une réponse HTTP 200 OK avec le contenu demandé selon le contexte de récupération s'il existe bien,
        /// sinon retourne une réponse HTTP 404 avec un message d'erreur
        /// </summary>
        /// <typeparam name="T">Type des entités de la liste fournie</typeparam>
        /// <param name="entityList">Liste des entités à récupérer dans la requête entrante</param>
        /// <param name="retrievalContext">Contexte de récupération (unique ou multiple)</param>
        /// <returns></returns>
        protected ActionResult RetrievalResponse<TCollection, TEntity>(List<TEntity> entityList, RetrievalContext retrievalContext)
        {
            if (entityList == null) 
                return GetRetrievalResponse<TCollection>(null, retrievalContext);

            return GetRetrievalResponse(entityList, retrievalContext);
        }

        protected ActionResult RetrievalResponse<TEntity>(List<TEntity> entityList, RetrievalContext retrievalContext)
            => GetRetrievalResponse(entityList, retrievalContext);

        private ActionResult GetRetrievalResponse<T>(List<T> entityList, RetrievalContext retrievalContext)
        {
            if (IsEmptyEntityList(entityList))
            {
                ProblemDescription notFoundProblemDescription = GetProblemDescription<T>(HttpStatusCode.NotFound);
                return NotFound(notFoundProblemDescription);
            }
            return retrievalContext == RetrievalContext.Single ? Ok(entityList.First()) : Ok(entityList);
        }

        /// <summary>
        /// Détermine si une liste d'entités retournées en réponse à la requête entrante est vide ou non
        /// </summary>
        /// <typeparam name="T">Type des entités de la liste</typeparam>
        /// <param name="entityList">Liste des entités récupérées</param>
        /// <returns></returns>
        private bool IsEmptyEntityList<T>(List<T> entityList)
            => entityList == null || entityList.Count == 0 || entityList.All(entity => entity == null);

        /// <summary>
        /// Retourne une instance de ProblemDescription avec un message d'erreur et un code de statut HTTP
        /// </summary>
        /// <param name="type">Type de l'entité ciblée</param>
        /// <param name="statusCode">Statut HTTP</param>
        /// <returns></returns>
        private ProblemDescription GetProblemDescription<T>(HttpStatusCode statusCode)
        {
            ProblemDescription problemDescription = _problemDescriptionService
                .GetNotFoundProblemDescription<T>(statusCode, HttpContext);

            return problemDescription;
        }
    }
}
