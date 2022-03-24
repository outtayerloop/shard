using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Shard.WiemEtBrunelle.Web.Constants;
using Shard.WiemEtBrunelle.Web.Models.Problem;
using System.Net;

namespace Shard.WiemEtBrunelle.Web.Services.Problem
{
    public class ProblemDescriptionService : IProblemDescriptionService
    {

        public ProblemDescription GetNotFoundProblemDescription<T>(HttpStatusCode statusCode, HttpContext context)
        {
            string errorMessage = GetEntityNotFoundMessage<T>();
            string contextUrl = context.Request.GetEncodedUrl();
            ProblemDescription problemDescription = new ProblemDescription(statusCode, errorMessage, contextUrl);
            return problemDescription;
        }

        public ProblemDescription GetBadRequestProblemDescription(HttpContext context, string badRequestMessage)
        {
            string contextUrl = context.Request.GetEncodedUrl();
            ProblemDescription problemDescription = new ProblemDescription(HttpStatusCode.BadRequest, badRequestMessage, contextUrl);
            return problemDescription;
        }

        private string GetEntityNotFoundMessage<T>()
        {
            string entityTranslatedName = EntityNotFoundConstants.EntityNamePairs[typeof(T).Name];
            string errorMessage = $"{entityTranslatedName} introuvable.";
            return errorMessage;
        }
    }
}
