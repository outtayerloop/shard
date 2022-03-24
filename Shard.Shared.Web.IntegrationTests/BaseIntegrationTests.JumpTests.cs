using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;

namespace Shard.Shared.Web.IntegrationTests
{
    public partial class BaseIntegrationTests<TEntryPoint, TWebApplicationFactory>
    {
        private string jumpingUserId = Guid.NewGuid().ToString();
        private string jumpingUnitId = Guid.NewGuid().ToString();
        private DateTimeOffset jumpingUserDateOfCreation = DateTimeOffset.Now.AddYears(-5);

        private async Task<JObject> ReceivingNewUser_BaseScenario()
        {
            using var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = CreateShardAuthorizationHeader(
                "fake-remote", "caramba");

            var response = await client.PutAsJsonAsync($"users/{jumpingUserId}", new
            {
                id = jumpingUserId,
                pseudo = "remote.user",
                dateOfCreation = jumpingUserDateOfCreation
            });

            await response.AssertSuccessStatusCode();
            return await response.Content.ReadAsAsync<JObject>();
        }

        private async Task<JObject> ReceivingJumpingCargo_BaseScenario()
        {
            await ReceivingNewUser_BaseScenario();

            using var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = CreateShardAuthorizationHeader(
                "fake-remote", "caramba");

            var response = await client.PutAsJsonAsync($"users/{jumpingUserId}/units/{jumpingUnitId}", new
            {
                id = jumpingUnitId,
                type = "cargo",
                health = 5,
                resourcesQuantity = new
                {
                    water = 12,
                    oxygen = 16
                }
            });

            await response.AssertSuccessStatusCode();
            return await response.Content.ReadAsAsync<JObject>();
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "6")]
        public async Task ReceivingNewUser_Works()
        {
            using var client = factory.CreateClient();
            await ReceivingNewUser_BaseScenario();
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "6")]
        public async Task ReceivingNewUser_CreatesUserWithCorrectInfo()
        {
            using var client = factory.CreateClient();
            var user = await ReceivingNewUser_BaseScenario();

            Assert.Equal(jumpingUserId, user["id"].Value<string>());
            Assert.Equal("remote.user", user["pseudo"].Value<string>());
            Assert.Equal(jumpingUserDateOfCreation, user["dateOfCreation"].Value<DateTime>());

            using var getUserResponse = await client.GetAsync($"users/{jumpingUserId}");
            var user2 = await getUserResponse.Content.ReadAsAsync<JObject>();
            Assert.Equal(user.ToString(), user2.ToString());
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "6")]
        public async Task ReceivingNewUser_CreatesUserWithNoUnitNorResources()
        {
            using var client = factory.CreateClient();
            var user = await ReceivingNewUser_BaseScenario();

            var resourceKinds = new[]
            {
                "carbon",
                "iron",
                "gold",
                "aluminium",
                "titanium",
                "water",
                "oxygen",
            };

            foreach (var resourceKind in resourceKinds)
                Assert.Equal(0, user["resourcesQuantity"][resourceKind].Value<int>());

            using var getUnitResponses = await client.GetAsync($"users/{jumpingUserId}/units");
            await getUnitResponses.AssertSuccessStatusCode();

            var units = (await getUnitResponses.Content.ReadAsAsync<JArray>()).ToArray();
            Assert.Empty(units);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "6")]
        public async Task ReceivingJumpingCargo_Works()
        {
            using var client = factory.CreateClient();
            await ReceivingJumpingCargo_BaseScenario();
        }
        
        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "6")]
        public async Task ReceivingJumpingCargo_UnitIsSoleUnitOfUser()
        {
            await ReceivingJumpingCargo_BaseScenario();

            using var client = factory.CreateClient();

            using var getUnitResponses = await client.GetAsync($"users/{jumpingUserId}/units");
            await getUnitResponses.AssertSuccessStatusCode();

            var units = (await getUnitResponses.Content.ReadAsAsync<JArray>()).ToArray();
            Assert.Single(units);
            Assert.Equal(jumpingUnitId, units[0]["id"].Value<string>());
            Assert.Equal("80ad7191-ef3c-14f0-7be8-e875dad4cfa6", units[0]["system"].Value<string>());
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "6")]
        public async Task ReceivingJumpingCargo_UnitLandInExpectedSystem()
        {
            await ReceivingJumpingCargo_BaseScenario();

            using var client = factory.CreateClient();

            using var getUnitResponses = await client.GetAsync($"users/{jumpingUserId}/units");
            await getUnitResponses.AssertSuccessStatusCode();

            var units = (await getUnitResponses.Content.ReadAsAsync<JArray>()).ToArray();
            Assert.Single(units);
            Assert.Equal("80ad7191-ef3c-14f0-7be8-e875dad4cfa6", units[0]["system"].Value<string>());
            Assert.True(!((JObject)units[0]).ContainsKey("planet") || units[0]["planet"].Type == JTokenType.Null);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "6")]
        public async Task ReceivingJumpingCargo_UnitKeepsResourcesAndHealthPoints()
        {
            await ReceivingJumpingCargo_BaseScenario();

            using var client = factory.CreateClient();

            using var getUnitResponses = await client.GetAsync($"users/{jumpingUserId}/units");
            await getUnitResponses.AssertSuccessStatusCode();

            var units = (await getUnitResponses.Content.ReadAsAsync<JArray>()).ToArray();
            Assert.Single(units);
            Assert.Equal(5, units[0]["health"].Value<int>());
            Assert.Equal(12, units[0]["resourcesQuantity"]["water"].Value<int>());
            Assert.Equal(16, units[0]["resourcesQuantity"]["oxygen"].Value<int>());
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "6")]
        public async Task JumpingCargo_RedirectsToTheUri()
        {
            var (response, userEntry, unitEntry) = await JumpingCargo_StandardScenario();

            await response.AssertStatusEquals(HttpStatusCode.PermanentRedirect);
            Assert.Equal(unitEntry.ExpectedUri, response.Headers.Location.ToString());
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "6")]
        public async Task JumpingCargo_SendsUserDetails()
        {
            var (response, userEntry, unitEntry) = await JumpingCargo_StandardScenario();

            var userBody = await userEntry.ActualRequestContent.ReadAsAsync<JObject>();
            Assert.Equal(userEntry.ExpectedUri.Split('/').Last(), userBody["id"].Value<string>());
            Assert.Equal("johny", userBody["pseudo"].Value<string>());
            Assert.True(userBody.ContainsKey("dateofcreation"));
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "6")]
        public async Task JumpingCargo_SendsUnitDetails()
        {
            var (response, userEntry, unitEntry) = await JumpingCargo_StandardScenario();

            var unitBody = await unitEntry.ActualRequestContent.ReadAsAsync<JObject>();
            Assert.Equal(unitEntry.ExpectedUri.Split('/').Last(), unitBody["id"].Value<string>());
            Assert.Equal("cargo", unitBody["type"].Value<string>());
            Assert.Equal(100, unitBody["health"].Value<int>());
            Assert.Equal(15, unitBody["resourcesquantity"]["water"].Value<int>());
            Assert.Equal(27, unitBody["resourcesquantity"]["oxygen"].Value<int>());
        }

        private async Task<(HttpResponseMessage response, FakeHttpHandler.Entry userEntry, FakeHttpHandler.Entry unitEntry)> 
            JumpingCargo_StandardScenario()
        {
            using var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions()
            {
                AllowAutoRedirect = false
            });
            var (userPath, unitPath, unitId) = await CreateTransportReadyToJump(client);

            var userEntry = httpHandler.AddHandler(HttpMethod.Put, "http://10.0.0.42/" + userPath, HttpStatusCode.OK, null);
            var unitEntry = httpHandler.AddHandler(HttpMethod.Put, "http://10.0.0.42/" + unitPath, HttpStatusCode.OK, null);

            var jumpingResponse = await client.PutAsJsonAsync(unitPath, new
            {
                id = unitId,
                destinationShard = "fake-remote"
            });
            return (jumpingResponse, userEntry, unitEntry);
        }

        private async Task<(string userPath, string unitPath, string unitId)> CreateTransportReadyToJump(HttpClient client)
        {
            var (userPath, unitPath, unitId) = await CreateTransportAndLoadScenario(client);
            using var response = await client.PutAsJsonAsync(unitPath, new
            {
                id = unitId,
                destinationSystem = "80ad7191-ef3c-14f0-7be8-e875dad4cfa6"
            });
            await response.AssertSuccessStatusCode();

            await fakeClock.Advance(TimeSpan.FromHours(1)); 
            return (userPath, unitPath, unitId);
        }
    }
}
