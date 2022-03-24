using Microsoft.AspNetCore.Mvc.Testing;
using Shard.Shared.Web.IntegrationTests;
using Shard.WiemEtBrunelle.Web;

namespace Shard.WiemEtBrunelle.IntegrationTests
{
    public class IntegrationTests : BaseIntegrationTests<Startup, WebApplicationFactory<Startup>>
    {

        public IntegrationTests(WebApplicationFactory<Startup> factory) : base(factory)
        {

        }
    }
}
