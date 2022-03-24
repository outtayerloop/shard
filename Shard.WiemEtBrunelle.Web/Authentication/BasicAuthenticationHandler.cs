using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Shard.WiemEtBrunelle.Web.Constants.Requests;
using Microsoft.Extensions.Configuration;

namespace Shard.WiemEtBrunelle.Web.Authentication
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {

        private readonly IConfiguration _configuration;

        public BasicAuthenticationHandler(IConfiguration configuration, IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
                => _configuration = configuration;

           
        public readonly string MethodPut = "PUT";
        public readonly string MethodPost = "POST";
        public readonly string ActionForCreateOrUpdateUser = "CreateOrUpdateUser";
        public readonly string ActionForUpdateSingleUnit = "UpdateSingleUnitFromUser";
        public readonly string ActionForAddUnitToQueue = "QueuingUnit";
        public readonly string ActionProperty = "action";

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
            => Task.FromResult(HandleAuthenticate());

 
        private AuthenticateResult HandleAuthenticate()
        {
            
            if (!IsThereHeaderAuthorization())
            { 
                return AuthenticateResult.Fail(RequestConstants.MissingHeaderAuthorization); 
            }


            var credentials = GetAuthenticationInformations();

            if (DoAdminAuthenticate(credentials.username, credentials.password) || DoNewUserAuthenticate(credentials.username, credentials.password))
            {
                return ReturnTicketAfterAuthentication(credentials.username, credentials.methodName, credentials.actionName.ToString());
            }
            

            return AuthenticateResult.NoResult();
        }


        private bool IsThereHeaderAuthorization()
            => Request.Headers.ContainsKey(RequestConstants.AuthorizationHeaderName);

        private bool DoAdminAuthenticate(string username, string password)
            => username == _configuration.GetValue<string>("Administrator:login") && password == _configuration.GetValue<string>("Administrator:password");

        private bool AdminWantUpdateUser(string methodName, string actionName)
            => methodName == MethodPut && actionName.ToString() == ActionForCreateOrUpdateUser;

        private bool AdminWantPutUnit(string methodName, string actionName)
            => methodName == MethodPut && actionName.ToString() == ActionForUpdateSingleUnit;

        private bool AdminWantAddUnitToQueue(string methodName, string actionName)
            => methodName == MethodPost && actionName.ToString() == ActionForUpdateSingleUnit;

        private void ChangeAdministratorStatusToAuthenticated()
            =>RequestConstants.AdminIsAuthenticated = true;

        private void ChangeNewUserStatusToAuthenticated()
            => RequestConstants.NewUserIsAuthenticated = true;

        private bool DoNewUserAuthenticate(string username, string password)
            => password == _configuration.GetValue<string>($"Wormholes:{username}:sharedPassword");

        private AuthenticateResult ReturnTicketAfterAuthentication(string username, string methodName, string actionName)
        {
            if (AdminWantUpdateUser(methodName,actionName.ToString()) 
                || AdminWantPutUnit(methodName, actionName.ToString())
                || AdminWantAddUnitToQueue(methodName, actionName.ToString()))
            {
                if (username == _configuration.GetValue<string>("Administrator:login"))
                    ChangeAdministratorStatusToAuthenticated();
                else 
                    ChangeNewUserStatusToAuthenticated();
            }



            return CreateTicketAfterAuthentication(username);
        }

        private AuthenticateResult CreateTicketAfterAuthentication(string username)
        {
            var identity = new GenericIdentity(username, Scheme.Name);
            var principal = new GenericPrincipal(identity, new[] { _configuration.GetValue<string>("Administrator:login")});
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }

        private (string username, string password, string actionName, string methodName) GetAuthenticationInformations()
        {

            var authorizationHeader = AuthenticationHeaderValue.Parse(Request.Headers[RequestConstants.AuthorizationHeaderName]);
            var credentialsBytes = Convert.FromBase64String(authorizationHeader.Parameter);
            var credentials = Encoding.UTF8.GetString(credentialsBytes).Split(':');

            if (credentials.Length != 2)
                return (null, null, null, null);
            else
            {
                string username = GetProvidedUsername(credentials);

                var password = credentials[1];

                var actionName = Request.RouteValues[ActionProperty];
                var methodName = Request.Method;

                return (username, password, actionName.ToString(), methodName);
            }
        }

        private string GetProvidedUsername(string[] providedCredentials)
        {
            string[] usernameComponents = providedCredentials[0].Split("shard-");

            return usernameComponents.Length == 2
                ? usernameComponents[1] // Authentification d'un utilisateur distant
                : providedCredentials[0]; // Authentification de l'administrateur local
        }

    }
}
