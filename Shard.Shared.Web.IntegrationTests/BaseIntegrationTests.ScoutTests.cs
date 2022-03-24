using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Shard.Shared.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Shard.Shared.Web.IntegrationTests
{
    public partial class BaseIntegrationTests<TEntryPoint, TWebApplicationFactory>
    {
        private async Task<string> CreateNewUserPath()
        {
            var userId = Guid.NewGuid().ToString();
            using var client = factory.CreateClient();
            using var userCreationResponse = await client.PutAsJsonAsync("users/" + userId, new
            {
                id = userId,
                pseudo = "johny"
            });
            await userCreationResponse.AssertSuccessStatusCode();

            return "users/" + userId;
        }

        private Task<JObject> GetScout(string userPath)
            => GetSingleUnitOfType(userPath, "scout");

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "2")]
        public Task CreatingUserCreatesScout()
            => CreatingUserCreatesOneUnitOfType("scout");

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "2")]
        public async Task CreatingUserCreatesScoutInSomeSystem()
        {
            var unit = await GetScout(await CreateNewUserPath());
            Assert.NotNull(unit["system"]);
            Assert.Equal(JTokenType.String, unit["system"].Type);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "2")]
        public async Task CreatingUserCreatesScoutInSomeExistingSystem()
        {
            var unit = await GetScout(await CreateNewUserPath());
            var systemName = unit["system"].Value<string>();

            using var client = factory.CreateClient();
            using var response = await client.GetAsync("systems");
            await response.AssertSuccessStatusCode();

            var systems = await response.Content.ReadAsAsync<JArray>();
            var system = systems.SingleOrDefault(system => system["name"].Value<string>() == systemName);
            Assert.NotNull(system);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "2")]
        public Task GettingScoutStatusById()
            => GettingUnitStatusById("scout");

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "2")]
        public Task GettingScoutStatusWithWrongIdReturns404()
            => GettingUnitStatusWithWrongIdReturns404("scout");

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "5")]
        public Task PutNonExistingScoutAsUnauthenticated()
            => PutNonExistingUnitAsUnauthenticated("scout");

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "5")]
        public Task PutNonExistingScoutAsAdministrator()
            => PutNonExistingUnitAsAdministrator("scout");

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "2")]
        public Task MoveScoutToOtherSystem()
            => MoveUnitToOtherSystem("scout");

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "2")]
        public Task MoveScoutToPlanet()
            => MoveUnitToPlanet("scout");

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "2")]
        public async Task AskingCurrentLocationsReturnsDetails()
        {
            var userPath = await CreateNewUserPath();
            var unit = await GetScout(userPath);
            var unitId = unit["id"].Value<string>();

            var currentSystem = unit["system"].Value<string>();
            var destinationPlanet = await GetSomePlanetInSystem(currentSystem);

            using var client = factory.CreateClient();
            using var moveResponse = await client.PutAsJsonAsync($"{userPath}/units/{unitId}", new
            {
                id = unitId,
                destinationSystem = currentSystem,
                destinationPlanet
            });
            await moveResponse.AssertSuccessStatusCode();

            await fakeClock.Advance(new TimeSpan(0, 0, 15));

            using var scoutingResponse = await client.GetAsync($"{userPath}/units/{unitId}/location");
            await scoutingResponse.AssertSuccessStatusCode();

            var location = await scoutingResponse.Content.ReadAsAsync<JObject>();
            Assert.Equal(currentSystem, location["system"].Value<string>());
            Assert.Equal(destinationPlanet, location["planet"].Value<string>());

            AssertResourcesQuantity(location);
        }

        private static void AssertResourcesQuantity(JObject data)
        {
            IDictionary<string, JToken> resources = data["resourcesQuantity"].Value<JObject>();
            Assert.NotNull(resources);

            foreach (var key in resources.Keys)
            {
                Assert.Contains(key, new[]
                {
                    "carbon",
                    "iron",
                    "gold",
                    "aluminium",
                    "titanium",
                    "water",
                    "oxygen",
                });
            }
        }

        [Fact]
        [Trait("grading", "true")]
        public Task GetScout_IfMoreThan2secAway_DoesNotWait()
            => GetUnit_IfMoreThan2secAway_DoesNotWait("scout");

        [Fact]
        [Trait("grading", "true")]
        public Task GetScout_IfLessOrEqualThan2secAway_Waits()
            => GetUnit_IfLessOrEqualThan2secAway_Waits("scout");

        [Fact]
        [Trait("grading", "true")]
        public Task GetScout_IfLessOrEqualThan2secAway_WaitsUntilArrived()
            => GetUnit_IfLessOrEqualThan2secAway_WaitsUntilArrived("scout");
    }
}
