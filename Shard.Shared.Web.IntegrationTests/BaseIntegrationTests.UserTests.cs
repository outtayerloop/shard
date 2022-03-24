using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Shard.Shared.Web.IntegrationTests
{
    public partial class BaseIntegrationTests<TEntryPoint, TWebApplicationFactory>
    {
        
        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "2")]
        public async Task CanGet404WhenQueryingUser()
        {
            using var client = factory.CreateClient();
            using var response = await client.GetAsync("users/42");
            await response.AssertStatusEquals(HttpStatusCode.NotFound);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "2")]
        public async Task CanCreateUser()
        {
            using var client = factory.CreateClient();
            using var response = await client.PutAsJsonAsync("users/43", new
            {
                id = "43",
                pseudo = "johny"
            });
            await response.AssertSuccessStatusCode();

            var user = await response.Content.ReadAsAsync<JObject>();
            Assert.Equal("43", user["id"].Value<string>());
            Assert.Equal("johny", user["pseudo"].Value<string>());
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "2")]
        public async Task CreatingUserWithInconsistentIdFails()
        {
            using var client = factory.CreateClient();
            using var response = await client.PutAsJsonAsync("users/44", new
            {
                id = "45",
                pseudo = "johny"
            });
            await response.AssertStatusEquals(HttpStatusCode.BadRequest);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "2")]
        public async Task CreatingUserWithLackOfBodyFails()
        {
            using var client = factory.CreateClient();
            using var response = await client.PutAsJsonAsync<object>("users/46", null);
            await response.AssertStatusEquals(HttpStatusCode.BadRequest);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "2")]
        public async Task CreatingUserWithInvalidIdFails()
        {
            using var client = factory.CreateClient();
            using var response = await client.PutAsJsonAsync("users/'", new
            {
                id = "'",
                pseudo = "johny"
            });
            await response.AssertStatusCodeAmong(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "2")]
        public async Task CanFetchCreatedUser()
        {
            using var client = factory.CreateClient();
            using var userCreationResponse = await client.PutAsJsonAsync("users/47", new
            {
                id = "47",
                pseudo = "johny"
            });
            await userCreationResponse.AssertSuccessStatusCode();

            using var getUserResponse = await client.GetAsync("users/47");
            await getUserResponse.AssertSuccessStatusCode();

            var user = await getUserResponse.Content.ReadAsAsync<JObject>();
            Assert.NotNull(user["id"]);
            Assert.Equal(JTokenType.String, user["id"].Type);
            Assert.Equal("47", user["id"].Value<string>());

            Assert.NotNull(user["pseudo"]);
            Assert.Equal(JTokenType.String, user["pseudo"].Type);
            Assert.Equal("johny", user["pseudo"].Value<string>());
        }
        
        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "3")]
        public async Task CanFetchResourcesFromNewlyCreatedUser()
        {
            using var client = factory.CreateClient();
            using var getUserResponse = await client.GetAsync(await CreateNewUserPath());

            var user = await getUserResponse.Content.ReadAsAsync<JObject>();
            AssertResourcesQuantity(user);
        }

        [Theory]
        [InlineData("aluminium", 0)]
        [InlineData("carbon", 20)]
        [InlineData("gold", 0)]
        [InlineData("iron", 10)]
        [InlineData("oxygen", 50)]
        [InlineData("titanium", 0)]
        [InlineData("water", 50)]
        [Trait("grading", "true")]
        [Trait("version", "3")]
        public async Task GivesBasicResourcesToNewUser(string resourceName, int resourceQuantity)
        {
            using var client = factory.CreateClient();
            var userPath = await CreateNewUserPath();

            await AssertResourceQuantity(client, userPath, resourceName, resourceQuantity);
        }



        private static async Task AssertResourceQuantity(HttpClient client, string userPath, string resourceName, int resourceQuantity)

        {

            var getUserResponse = await client.GetAsync(userPath);

            var user = await getUserResponse.Content.ReadAsAsync<JObject>();

            Assert.Equal(resourceQuantity, user["resourcesQuantity"][resourceName].Value<int>());

        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "5")]
        public async Task CanForceResourcesForUser()
        {
            using var client = factory.CreateClient();

            var userPath = await CreateNewUserPath();

            var user = await (await client.GetAsync(userPath)).Content.ReadAsAsync<JObject>();



            using var response = await PutResources(userPath, new

            {

                aluminium = 421,

                carbon = 422,

                gold = 423,

                iron = 424,

                oxygen = 425,

                titanium = 426,

                water = 427
            });

            await response.AssertSuccessStatusCode();

            var updatedUser = await response.Content.ReadAsAsync<JObject>();

            Assert.Equal(user["id"].Value<string>(), updatedUser["id"].Value<string>());

            Assert.Equal(user["pseudo"].Value<string>(), updatedUser["pseudo"].Value<string>());

            Assert.Equal(user["dateOfCreation"].Value<string>(), updatedUser["dateOfCreation"].Value<string>());



            Assert.Equal(421, updatedUser["resourcesQuantity"]["aluminium"].Value<int>());

            Assert.Equal(422, updatedUser["resourcesQuantity"]["carbon"].Value<int>());

            Assert.Equal(423, updatedUser["resourcesQuantity"]["gold"].Value<int>());

            Assert.Equal(424, updatedUser["resourcesQuantity"]["iron"].Value<int>());

            Assert.Equal(425, updatedUser["resourcesQuantity"]["oxygen"].Value<int>());

            Assert.Equal(426, updatedUser["resourcesQuantity"]["titanium"].Value<int>());

            Assert.Equal(427, updatedUser["resourcesQuantity"]["water"].Value<int>());

        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "5")]
        public async Task CanForceSomeResourcesForUser()
        {
            using var client = factory.CreateClient();

            var userPath = await CreateNewUserPath();


            using var response = await PutResources(userPath, new

            {

                carbon = 422,

                gold = 423,
            });

            await response.AssertSuccessStatusCode();

            var updatedUser = await response.Content.ReadAsAsync<JObject>();

            Assert.Equal(0, updatedUser["resourcesQuantity"]["aluminium"].Value<int>());

            Assert.Equal(422, updatedUser["resourcesQuantity"]["carbon"].Value<int>());

            Assert.Equal(423, updatedUser["resourcesQuantity"]["gold"].Value<int>());

            Assert.Equal(10, updatedUser["resourcesQuantity"]["iron"].Value<int>());

            Assert.Equal(50, updatedUser["resourcesQuantity"]["oxygen"].Value<int>());

            Assert.Equal(0, updatedUser["resourcesQuantity"]["titanium"].Value<int>());

            Assert.Equal(50, updatedUser["resourcesQuantity"]["water"].Value<int>());

        }



        private async Task<HttpResponseMessage> PutResources(string userPath, object resources)

        {
            using var client = factory.CreateClient();

            client.DefaultRequestHeaders.Authorization = CreateAdminAuthorizationHeader();



            var user = await (await client.GetAsync(userPath)).Content.ReadAsAsync<JObject>();

            return await client.PutAsJsonAsync(userPath, new
            {
                id = user["id"].Value<string>(),
                resourcesQuantity = resources
            });
        }

        private static AuthenticationHeaderValue CreateShardAuthorizationHeader(string shardName, string sharedKey)
        {
            return new AuthenticationHeaderValue(
                "Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"shard-{shardName}:{sharedKey}")));
        }

        private static AuthenticationHeaderValue CreateAdminAuthorizationHeader()
        {
            return new AuthenticationHeaderValue(
                "Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes("admin:password")));
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "5")]
        public async Task IgnoreResourcesUpdateIfNotAdmin()
        {
            using var client = factory.CreateClient();



            var userPath = await CreateNewUserPath();

            var user = await (await client.GetAsync(userPath)).Content.ReadAsAsync<JObject>();

            using var response = await client.PutAsJsonAsync(userPath, new

            {

                id = user["id"].Value<string>(),

                resourcesQuantity = new

                {

                    carbon = 422,

                    gold = 423,
                }

            });

            await response.AssertSuccessStatusCode();

            var updatedUser = await response.Content.ReadAsAsync<JObject>();

            Assert.Equal(user["id"].Value<string>(), updatedUser["id"].Value<string>());

            Assert.Equal(user["pseudo"].Value<string>(), updatedUser["pseudo"].Value<string>());

            Assert.Equal(user["dateOfCreation"].Value<string>(), updatedUser["dateOfCreation"].Value<string>());



            Assert.Equal(0, updatedUser["resourcesQuantity"]["aluminium"].Value<int>());

            Assert.Equal(20, updatedUser["resourcesQuantity"]["carbon"].Value<int>());

            Assert.Equal(0, updatedUser["resourcesQuantity"]["gold"].Value<int>());

            Assert.Equal(10, updatedUser["resourcesQuantity"]["iron"].Value<int>());

            Assert.Equal(50, updatedUser["resourcesQuantity"]["oxygen"].Value<int>());

            Assert.Equal(0, updatedUser["resourcesQuantity"]["titanium"].Value<int>());

            Assert.Equal(50, updatedUser["resourcesQuantity"]["water"].Value<int>());

        }

    }
}
