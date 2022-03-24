using Microsoft.AspNetCore.Http;
using Shard.WiemEtBrunelle.Web.Models.Problem;
using System.Net;

namespace Shard.WiemEtBrunelle.Web.Services.Problem
{
    public interface IProblemDescriptionService
    {
        ProblemDescription GetNotFoundProblemDescription<T>(HttpStatusCode statusCode, HttpContext context);

        ProblemDescription GetBadRequestProblemDescription(HttpContext context, string badRequest);
    }
}
