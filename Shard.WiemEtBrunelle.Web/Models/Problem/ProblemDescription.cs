using System.Net;

namespace Shard.WiemEtBrunelle.Web.Models.Problem
{
    public class ProblemDescription
    {

        public ProblemDescription(HttpStatusCode statusCode, string errorMessage, string contextUrl)
        {
            StatusCode = statusCode;
            ErrorMessage = errorMessage;
            ContextUrl = contextUrl;
        }

        public HttpStatusCode StatusCode { get; }

        public string ErrorMessage { get; }

        public string ContextUrl { get; }
    }
}
