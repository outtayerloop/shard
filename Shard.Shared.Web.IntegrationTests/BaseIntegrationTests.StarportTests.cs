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
        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "5")]
        public async Task BuildingStarportThenFetchingAllBuildingsIncludesStarport()
        {
            using var client = factory.CreateClient();
            var (userPath, _, originalBuilding) = await BuildStarport(client);

            var response = await client.GetAsync($"{userPath}/buildings");
            await response.AssertSuccessStatusCode();

            var buildings = (await response.Content.ReadAsAsync<JArray>()).ToArray();
            Assert.Single(buildings);
            var building = buildings[0].Value<JObject>();

            Assert.Equal(originalBuilding.ToString(), building.ToString());
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "5")]
        public async Task BuildingStarportThenFetchingBuildingByIdReturnsStarport()
        {
            using var client = factory.CreateClient();
            var (userPath, _, originalBuilding) = await BuildStarport(client);
            var building = await RefreshBuilding(client, userPath, originalBuilding);

            Assert.Equal(originalBuilding.ToString(), building.ToString());
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "5")]
        public async Task BuildingStarportThenWaiting4MinReturnsUnbuiltStarport()
        {
            using var client = factory.CreateClient();
            var (userPath, _, originalBuilding) = await BuildStarport(client);

            await fakeClock.Advance(TimeSpan.FromMinutes(4));
            var building = await RefreshBuilding(client, userPath, originalBuilding);

            Assert.False(building["isBuilt"].Value<bool>());
            Assert.Equal(fakeClock.Now.AddMinutes(1), building["estimatedBuildTime"].Value<DateTime>());
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "5")]
        public async Task BuildingStarportThenWaiting5MinReturnsBuiltStarport()
        {
            using var client = factory.CreateClient();
            var (userPath, _, originalBuilding) = await BuildStarport(client);

            await fakeClock.Advance(TimeSpan.FromMinutes(5));
            var building = await RefreshBuilding(client, userPath, originalBuilding);

            Assert.True(building["isBuilt"].Value<bool>());
            Assert.True(!building.ContainsKey("estimatedBuildTime")
                || building["estimatedBuildTime"].Type == JTokenType.Null);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "5")]
        public async Task QueuingScoutOnBuiltStarportImmediatlyReturnsOne()
        {
            using var client = factory.CreateClient();
            var (userPath, _, originalBuilding) = await BuildStarport(client);

            await fakeClock.Advance(TimeSpan.FromMinutes(5));
            var response = await client.PostAsJsonAsync($"{userPath}/buildings/{originalBuilding["id"].Value<string>()}/queue", new
            {
                type = "scout"
            });
            await response.AssertSuccessStatusCode();

            var unit = await response.Content.ReadAsAsync<JObject>();
            Assert.NotNull(unit["id"].Value<string>());
            Assert.Equal("scout", unit["type"].Value<string>());
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "5")]
        public async Task QueuingScoutOnBuiltStarportCost5Carbon5Iron()
        {
            using var client = factory.CreateClient();
            var (userPath, _, originalBuilding) = await BuildStarport(client);

            await fakeClock.Advance(TimeSpan.FromMinutes(5));

            await AssertResourceQuantity(client, userPath, "carbon", 20);
            await AssertResourceQuantity(client, userPath, "iron", 10);

            var response = await client.PostAsJsonAsync($"{userPath}/buildings/{originalBuilding["id"].Value<string>()}/queue", new
            {
                type = "scout"
            });
            await response.AssertSuccessStatusCode();
            await AssertResourceQuantity(client, userPath, "carbon", 15);
            await AssertResourceQuantity(client, userPath, "iron", 5);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "5")]
        public async Task QueuingBuilderOnBuiltStarportCost5Carbon10Iron()
        {
            using var client = factory.CreateClient();
            var (userPath, _, originalBuilding) = await BuildStarport(client);

            await fakeClock.Advance(TimeSpan.FromMinutes(5));

            await AssertResourceQuantity(client, userPath, "carbon", 20);
            await AssertResourceQuantity(client, userPath, "iron", 10);

            var response = await client.PostAsJsonAsync($"{userPath}/buildings/{originalBuilding["id"].Value<string>()}/queue", new
            {
                type = "builder"
            });
            await response.AssertSuccessStatusCode();
            await AssertResourceQuantity(client, userPath, "carbon", 15);
            await AssertResourceQuantity(client, userPath, "iron", 0);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "5")]
        public async Task QueuingScoutForInvalidUserReturns404()
        {
            using var client = factory.CreateClient();
            var (userPath, _, originalBuilding) = await BuildStarport(client);

            await fakeClock.Advance(TimeSpan.FromMinutes(5));
            var response = await client.PostAsJsonAsync($"{userPath}z/buildings/{originalBuilding["id"].Value<string>()}/queue", new
            {
                type = "scout"
            });
            await response.AssertStatusEquals(HttpStatusCode.NotFound);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "5")]
        public async Task QueuingScoutForInvalidBuildingReturns404()
        {
            using var client = factory.CreateClient();
            var (userPath, _, originalBuilding) = await BuildStarport(client);

            await fakeClock.Advance(TimeSpan.FromMinutes(5));
            var response = await client.PostAsJsonAsync($"{userPath}/buildings/{originalBuilding["id"].Value<string>()}z/queue", new
            {
                type = "scout"
            });
            await response.AssertStatusEquals(HttpStatusCode.NotFound);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "5")]
        public async Task QueuingScoutOnMineReturns400()
        {
            using var client = factory.CreateClient();
            var (userPath, _, originalBuilding) = await BuildMine(client);

            await fakeClock.Advance(TimeSpan.FromMinutes(5));
            var response = await client.PostAsJsonAsync($"{userPath}/buildings/{originalBuilding["id"].Value<string>()}/queue", new
            {
                type = "scout"
            });
            await response.AssertStatusEquals(HttpStatusCode.BadRequest);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "5")]
        public async Task QueuingScoutOnUnBuiltStarportReturns400()
        {
            using var client = factory.CreateClient();
            var (userPath, _, originalBuilding) = await BuildStarport(client);

            var response = await client.PostAsJsonAsync($"{userPath}/buildings/{originalBuilding["id"].Value<string>()}/queue", new
            {
                type = "scout"
            });
            await response.AssertStatusEquals(HttpStatusCode.BadRequest);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "5")]
        public async Task QueuingScoutIfNotEnoughResourcesReturns400()
        {
            using var client = factory.CreateClient();

            var (userPath, _, originalBuilding) = await BuildStarport(client);
            using var putResourceResponse = await PutResources(userPath, new
            {
                carbon = 0,
                iron = 0,
            });

            var response = await client.PostAsJsonAsync($"{userPath}/buildings/{originalBuilding["id"].Value<string>()}/queue", new
            {
                type = "scout"
            });
            await response.AssertStatusEquals(HttpStatusCode.BadRequest);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "5")]
        public async Task QueuingScoutIfNotEnoughIronDoesNotSpendCarbon()
        {
            using var client = factory.CreateClient();

            var (userPath, _, originalBuilding) = await BuildStarport(client);
            using var putResourceResponse = await PutResources(userPath, new
            {
                carbon = 20,
                iron = 0,
            });

            var response = await client.PostAsJsonAsync($"{userPath}/buildings/{originalBuilding["id"].Value<string>()}/queue", new
            {
                type = "scout"
            });
            await response.AssertStatusEquals(HttpStatusCode.BadRequest);
            await AssertResourceQuantity(client, userPath, "carbon", 20);
            await AssertResourceQuantity(client, userPath, "iron", 0);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "5")]
        public async Task QueuingScoutIfNotEnoughCarbonDoesNotSpendIron()
        {
            using var client = factory.CreateClient();

            var (userPath, _, originalBuilding) = await BuildStarport(client);
            using var putResourceResponse = await PutResources(userPath, new
            {
                carbon = 0,
                iron = 10,
            });

            var response = await client.PostAsJsonAsync($"{userPath}/buildings/{originalBuilding["id"].Value<string>()}/queue", new
            {
                type = "scout"
            });
            await response.AssertStatusEquals(HttpStatusCode.BadRequest);
            await AssertResourceQuantity(client, userPath, "carbon", 0);
            await AssertResourceQuantity(client, userPath, "iron", 10);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "6")]
        public async Task QueuingCargoOnBuiltStarportCosts10Carbon10Iron5Gold()
        {
            using var client = factory.CreateClient();
            var (userPath, _, originalBuilding) = await BuildStarport(client);

            await fakeClock.Advance(TimeSpan.FromMinutes(5));

            using var putResourceResponse = await PutResources(userPath, new
            {
                carbon = 10,
                iron = 10,
                gold = 5,
            });

            var response = await client.PostAsJsonAsync($"{userPath}/buildings/{originalBuilding["id"].Value<string>()}/queue", new
            {
                type = "cargo"
            });
            await response.AssertSuccessStatusCode();
            await AssertResourceQuantity(client, userPath, "carbon", 0);
            await AssertResourceQuantity(client, userPath, "iron", 0);
            await AssertResourceQuantity(client, userPath, "gold", 0);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "6")]
        public async Task CanLoadResourcesInCargo()
        {
            using var client = factory.CreateClient();
            var (_, unitPath, _) = await CreateTransportAndLoadScenario(client);

            var unit = await GetUnitOfPath(client, unitPath);

            Assert.NotNull(unit["resourcesQuantity"]);
            Assert.Equal(JTokenType.Object, unit["resourcesQuantity"].Type);
            Assert.Equal(15, unit["resourcesQuantity"]["water"].Value<int>());
            Assert.Equal(27, unit["resourcesQuantity"]["oxygen"].Value<int>());
        }

        private async Task<(string userPath, string unitPath, string unitId)> CreateTransportAndLoadScenario(HttpClient client,
            string unitType = "cargo", int waterLoaded = 15, bool shouldFail = false)
        {
            var (userPath, _, originalBuilding) = await BuildStarport(client);

            await fakeClock.Advance(TimeSpan.FromMinutes(5));

            using var putResourceResponse = await PutResources(userPath, new
            {
                carbon = 10,
                iron = 10,
                gold = 5,
                water = 25,
                oxygen = 30,
                aluminium = 11
            });
            await putResourceResponse.AssertSuccessStatusCode();

            var queuingRespponse = await client.PostAsJsonAsync($"{userPath}/buildings/{originalBuilding["id"].Value<string>()}/queue", new
            {
                type = unitType
            });
            await queuingRespponse.AssertSuccessStatusCode();

            var unit = await queuingRespponse.Content.ReadAsAsync<JObject>();
            string unitId = unit["id"].Value<string>();
            var unitPath = userPath + "/units/" + unitId;

            var loadingResponse = await client.PutAsJsonAsync(unitPath, new
            {
                id = unitId,
                resourcesQuantity = new
                {
                    water = waterLoaded,
                    oxygen = 27
                }
            });
            if (!shouldFail)
                await loadingResponse.AssertSuccessStatusCode();
            else
                await loadingResponse.AssertStatusEquals(HttpStatusCode.BadRequest);

            return (userPath, unitPath, unitId);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "6")]
        public async Task LoadingResourcesIntoCargoRemovesSaidResources()
        {
            using var client = factory.CreateClient();
            var (userPath, _, _) = await CreateTransportAndLoadScenario(client);

            await AssertResourceQuantity(client, userPath, "water", 10);
            await AssertResourceQuantity(client, userPath, "oxygen", 3);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "6")]
        public async Task CannotLoadResourcesInBuilder()
        {
            using var client = factory.CreateClient();
            await CreateTransportAndLoadScenario(client, 
                unitType: "builder", shouldFail: true);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "6")]
        public async Task CanUnloadSomeResourcesFromCargo()
        {
            using var client = factory.CreateClient();
            var (userPath, unitPath, unitId) = await CreateTransportAndLoadScenario(client);

            var unloadingResponse = await client.PutAsJsonAsync(unitPath, new
            {
                id = unitId,
                resourcesQuantity = new
                {
                    water = 10,
                    oxygen = 17
                }
            });

            var unit = await GetUnitOfPath(client, unitPath);

            Assert.NotNull(unit["resourcesQuantity"]);
            Assert.Equal(JTokenType.Object, unit["resourcesQuantity"].Type);
            Assert.Equal(10, unit["resourcesQuantity"]["water"].Value<int>());
            Assert.Equal(17, unit["resourcesQuantity"]["oxygen"].Value<int>());
            await AssertResourceQuantity(client, userPath, "water", 15);
            await AssertResourceQuantity(client, userPath, "oxygen", 13);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "6")]
        public async Task CanLoadAndUnloadSomeResourcesFromCargoAtTheSameTime()
        {
            using var client = factory.CreateClient();
            var (userPath, unitPath, unitId) = await CreateTransportAndLoadScenario(client);

            var unloadingResponse = await client.PutAsJsonAsync(unitPath, new
            {
                id = unitId,
                resourcesQuantity = new
                {
                    water = 5,
                    oxygen = 20,
                    aluminium = 9
                }
            });

            var unit = await GetUnitOfPath(client, unitPath);

            Assert.NotNull(unit["resourcesQuantity"]);
            Assert.Equal(JTokenType.Object, unit["resourcesQuantity"].Type);
            Assert.Equal(5, unit["resourcesQuantity"]["water"].Value<int>());
            Assert.Equal(20, unit["resourcesQuantity"]["oxygen"].Value<int>());
            Assert.Equal(9, unit["resourcesQuantity"]["aluminium"].Value<int>());
            await AssertResourceQuantity(client, userPath, "water", 20);
            await AssertResourceQuantity(client, userPath, "oxygen", 10);
            await AssertResourceQuantity(client, userPath, "aluminium", 2);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "6")]
        public async Task CannotLoadMoreResourcesThanUserHas()
        {
            using var client = factory.CreateClient();
            await CreateTransportAndLoadScenario(client,
                waterLoaded: 26, shouldFail: true);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "6")]
        public async Task CannotLoadResourcesIfNoStarport()
        {
            await CannotLoadOrUnloadResourcesIfNoStarport(new
            {
                water = 16,
                oxygen = 27
            });
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "6")]
        public async Task CannotUnLoadResourcesIfNoStarport()
        {
            await CannotLoadOrUnloadResourcesIfNoStarport(new
            {
                water = 12,
                oxygen = 27
            });
        }

        private async Task CannotLoadOrUnloadResourcesIfNoStarport(object resourcesToAssign)
        {
            using var client = factory.CreateClient();
            var (userPath, unitPath, unitId) = await CreateTransportAndLoadScenario(client);

            var unitBeforeMove = await GetUnitOfPath(client, unitPath);
            using var response = await client.PutAsJsonAsync(unitPath, new
            {
                id = unitId,
                destinationSystem = unitBeforeMove["system"].Value<string>()
            });
            await response.AssertSuccessStatusCode();

            var loadingResponse = await client.PutAsJsonAsync(unitPath, new
            {
                id = unitId,
                resourcesQuantity = resourcesToAssign
            });
            await loadingResponse.AssertStatusEquals(HttpStatusCode.BadRequest);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "6")]
        public async Task CanMoveUnitWithoutUnloadingResources()
        {
            using var client = factory.CreateClient();
            var (userPath, unitPath, unitId) = await CreateTransportAndLoadScenario(client);

            var unitBeforeMove = await GetUnitOfPath(client, unitPath);
            using var response = await client.PutAsJsonAsync(unitPath, new
            {
                id = unitId,
                destinationSystem = unitBeforeMove["system"].Value<string>()
            });
            await response.AssertSuccessStatusCode();

            var unit = await GetUnitOfPath(client, unitPath);

            Assert.NotNull(unit["resourcesQuantity"]);
            Assert.Equal(JTokenType.Object, unit["resourcesQuantity"].Type);
            Assert.Equal(15, unit["resourcesQuantity"]["water"].Value<int>());
            Assert.Equal(27, unit["resourcesQuantity"]["oxygen"].Value<int>());
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "6")]
        public async Task CanPutCargoWithoutChangingResourcesWithoutStarport()
        {
            using var client = factory.CreateClient();
            var (userPath, unitPath, unitId) = await CreateTransportAndLoadScenario(client);

            var unitBeforeMove = await GetUnitOfPath(client, unitPath);
            using var response = await client.PutAsJsonAsync(unitPath, new
            {
                id = unitId,
                destinationSystem = unitBeforeMove["system"].Value<string>()
            });
            await response.AssertSuccessStatusCode();

            var unloadingResponse = await client.PutAsJsonAsync(unitPath, new
            {
                id = unitId,
                resourcesQuantity = new
                {
                    water = 15,
                    oxygen = 27
                }
            });

            await unloadingResponse.AssertSuccessStatusCode();
            var unit = await GetUnitOfPath(client, unitPath);

            Assert.NotNull(unit["resourcesQuantity"]);
            Assert.Equal(JTokenType.Object, unit["resourcesQuantity"].Type);
            Assert.Equal(15, unit["resourcesQuantity"]["water"].Value<int>());
            Assert.Equal(27, unit["resourcesQuantity"]["oxygen"].Value<int>());
        }
    }
}
