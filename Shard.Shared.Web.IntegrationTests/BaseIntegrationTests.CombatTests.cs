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
        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "5")]
        public async Task FighterVsCruiser_5sec_Nothing()
        {
            await fakeClock.SetNow(DateTime.Today);

            using var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = CreateAdminAuthorizationHeader();

            var paths = await CreateDuel(client, "fighter", "cruiser");
            await fakeClock.Advance(TimeSpan.FromSeconds(5));
            var (fighterStatus, cruiserStatus) = await GetDuelStatus(client, paths);

            Assert.Equal(80, fighterStatus["health"].Value<int>());
            Assert.Equal(400, cruiserStatus["health"].Value<int>());
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "5")]
        public async Task FighterVsCruiser_6sec_FighterInflicts10dmg()
        {
            await fakeClock.SetNow(DateTime.Today);

            using var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = CreateAdminAuthorizationHeader();

            var (_, cruiserPath) = await CreateDuel(client, "fighter", "cruiser");
            await fakeClock.Advance(TimeSpan.FromSeconds(6));
            var cruiserStatus = await GetUnitOfPath(client, cruiserPath);

            Assert.Equal(390, cruiserStatus["health"].Value<int>());
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "5")]
        public async Task FighterVsCruiser_6sec_CruiserInflicts40dmg()
        {
            await fakeClock.SetNow(DateTime.Today);

            using var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = CreateAdminAuthorizationHeader();

            var (fighterPath, _) = await CreateDuel(client, "fighter", "cruiser");
            await fakeClock.Advance(TimeSpan.FromSeconds(6));
            var fighterStatus = await GetUnitOfPath(client, fighterPath);

            Assert.Equal(40, fighterStatus["health"].Value<int>());
        }



        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "5")]
        public async Task FighterVsBomber_5sec_Nothing()
        {
            await fakeClock.SetNow(DateTime.Today);

            using var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = CreateAdminAuthorizationHeader();

            var paths = await CreateDuel(client, "fighter", "bomber");
            await fakeClock.Advance(TimeSpan.FromSeconds(5));
            var (fighterStatus, bomberStatus) = await GetDuelStatus(client, paths);

            Assert.Equal(80, fighterStatus["health"].Value<int>());
            Assert.Equal(50, bomberStatus["health"].Value<int>());
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "5")]
        public async Task FighterVsBomber_6sec_FighterInflicts10dmg()
        {
            await fakeClock.SetNow(DateTime.Today);

            using var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = CreateAdminAuthorizationHeader();

            var (_, bomberPath) = await CreateDuel(client, "fighter", "bomber");
            await fakeClock.Advance(TimeSpan.FromSeconds(6));
            var bomberStatus = await GetUnitOfPath(client, bomberPath);

            Assert.Equal(40, bomberStatus["health"].Value<int>());
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "5")]
        public async Task FighterVsBomber_6sec_BomberInflicts0dmg()
        {
            await fakeClock.SetNow(DateTime.Today);

            using var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = CreateAdminAuthorizationHeader();

            var (fighterPath, _) = await CreateDuel(client, "fighter", "bomber");
            await fakeClock.Advance(TimeSpan.FromSeconds(6));
            var fighterStatus = await GetUnitOfPath(client, fighterPath);

            Assert.Equal(80, fighterStatus["health"].Value<int>());
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "5")]
        public async Task FighterVsBomber_6sec_atEndOFMinute_FighterInflicts10dmg()
        {
            await fakeClock.SetNow(DateTime.Today.AddSeconds(54));

            using var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = CreateAdminAuthorizationHeader();

            var (_, bomberPath) = await CreateDuel(client, "fighter", "bomber");
            await fakeClock.Advance(TimeSpan.FromSeconds(6));
            var bomberStatus = await GetUnitOfPath(client, bomberPath);

            Assert.Equal(40, bomberStatus["health"].Value<int>());
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "5")]
        public async Task FighterVsBomber_6sec_atEndOFMinute_BomberKillsFighter()
        {
            await fakeClock.SetNow(DateTime.Today.AddSeconds(54));

            using var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = CreateAdminAuthorizationHeader();

            var (fighterPath, _) = await CreateDuel(client, "fighter", "bomber");
            await fakeClock.Advance(TimeSpan.FromSeconds(6));

            await AssertPathNotFound(client, fighterPath);
        }



        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "5")]
        public async Task CruiserVsBomber_5sec_Nothing()
        {
            await fakeClock.SetNow(DateTime.Today);

            using var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = CreateAdminAuthorizationHeader();

            var paths = await CreateDuel(client, "cruiser", "bomber");
            await fakeClock.Advance(TimeSpan.FromSeconds(5));
            var (cruiserStatus, bomberStatus) = await GetDuelStatus(client, paths);

            Assert.Equal(400, cruiserStatus["health"].Value<int>());
            Assert.Equal(50, bomberStatus["health"].Value<int>());
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "5")]
        public async Task CruiserVsBomber_6sec_CruiserInflicts4dmg()
        {
            await fakeClock.SetNow(DateTime.Today);

            using var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = CreateAdminAuthorizationHeader();

            var (_, bomberPath) = await CreateDuel(client, "cruiser", "bomber");
            await fakeClock.Advance(TimeSpan.FromSeconds(6));
            var bomberStatus = await GetUnitOfPath(client, bomberPath);

            Assert.Equal(46, bomberStatus["health"].Value<int>());
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "5")]
        public async Task CruiserVsBomber_6sec_BomberInflicts0dmg()
        {
            await fakeClock.SetNow(DateTime.Today);

            using var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = CreateAdminAuthorizationHeader();

            var (cruiserPath, _) = await CreateDuel(client, "cruiser", "bomber");
            await fakeClock.Advance(TimeSpan.FromSeconds(6));
            var cruiserStatus = await GetUnitOfPath(client, cruiserPath);

            Assert.Equal(400, cruiserStatus["health"].Value<int>());
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "5")]
        public async Task CruiserVsBomber_60sec_CruiserInflicts60dmg()
        {
            await fakeClock.SetNow(DateTime.Today);

            using var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = CreateAdminAuthorizationHeader();

            var (_, bomberPath) = await CreateDuel(client, "cruiser", "bomber");
            await fakeClock.Advance(TimeSpan.FromSeconds(60));
            var bomberStatus = await GetUnitOfPath(client, bomberPath);

            Assert.Equal(10, bomberStatus["health"].Value<int>());
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "5")]
        public async Task CruiserVsBomber_60sec_BomberKillsFighter()
        {
            await fakeClock.SetNow(DateTime.Today);

            using var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = CreateAdminAuthorizationHeader();

            var (cruiserPath, _) = await CreateDuel(client, "cruiser", "bomber");
            await fakeClock.Advance(TimeSpan.FromSeconds(60));

            await AssertPathNotFound(client, cruiserPath);
        }



        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "5")]
        public async Task BomberVsBomber_KillsEachOtherAfter60Sec()
        {
            await fakeClock.SetNow(DateTime.Today);

            using var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = CreateAdminAuthorizationHeader();

            var paths = await CreateDuel(client, "bomber", "bomber");
            await fakeClock.Advance(TimeSpan.FromSeconds(59));
            var (bomber1, bomber2) = await GetDuelStatus(client, paths);

            Assert.Equal(50, bomber1["health"].Value<int>());
            Assert.Equal(50, bomber2["health"].Value<int>());

            await fakeClock.Advance(TimeSpan.FromSeconds(1));
            await AssertPathNotFound(client, paths.Item1);
            await AssertPathNotFound(client, paths.Item2);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "5")]
        public async Task FighterVsFighter_KillsEachOtherAfter48Sec()
        {
            await fakeClock.SetNow(DateTime.Today);

            using var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = CreateAdminAuthorizationHeader();

            var paths = await CreateDuel(client, "fighter", "fighter");
            await fakeClock.Advance(TimeSpan.FromSeconds(47));
            var (fighter1, fighter2) = await GetDuelStatus(client, paths);

            Assert.Equal(10, fighter1["health"].Value<int>());
            Assert.Equal(10, fighter2["health"].Value<int>());

            await fakeClock.Advance(TimeSpan.FromSeconds(1));
            await AssertPathNotFound(client, paths.Item1);
            await AssertPathNotFound(client, paths.Item2);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "5")]
        public async Task CruiserVsCruiser_KillsEachOtherAfter60Sec()
        {
            await fakeClock.SetNow(DateTime.Today);

            using var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = CreateAdminAuthorizationHeader();

            var paths = await CreateDuel(client, "cruiser", "cruiser");
            await fakeClock.Advance(TimeSpan.FromSeconds(59));
            var (cruiser1, cruiser2) = await GetDuelStatus(client, paths);

            Assert.Equal(40, cruiser1["health"].Value<int>());
            Assert.Equal(40, cruiser2["health"].Value<int>());

            await fakeClock.Advance(TimeSpan.FromSeconds(1));
            await AssertPathNotFound(client, paths.Item1);
            await AssertPathNotFound(client, paths.Item2);
        }


        private static readonly IReadOnlyDictionary<string, int> expectedHealthPoints = new Dictionary<string, int>()
        {
            { "cruiser", 400 },
            { "bomber", 50 },
            { "fighter", 80 },
        };

        [Theory]
        [Trait("grading", "true")]
        [Trait("version", "5")]
        [InlineData("cruiser", "cruiser", "fighter", "fighter")]
        [InlineData("cruiser", "cruiser", "bomber", "cruiser")]
        [InlineData("fighter", "cruiser", "fighter", "fighter")]
        [InlineData("fighter", "bomber", "fighter", "bomber")]
        [InlineData("bomber", "bomber", "fighter", "bomber")]
        [InlineData("bomber", "cruiser", "bomber", "cruiser")]
        public async Task CombatUnits_Priority(string soloType, string enemyType1, string enemyType2, string preferedType)
        {
            await fakeClock.SetNow(DateTime.Today.AddSeconds(54));

            using var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = CreateAdminAuthorizationHeader();
            var system = await GetRandomSystemOtherThan(null);

            await CreateUserWithUnit(client, soloType, system);
            var enemy1 = await CreateUserWithUnit(client, enemyType1, system);
            var enemy2 = await CreateUserWithUnit(client, enemyType2, system);

            await fakeClock.Advance(TimeSpan.FromSeconds(6));

            if (preferedType == enemyType1)
            {
                await AssertAttackedFirstAndNotSecond(client, enemy1, enemy2);
            }
            else
            {
                await AssertAttackedFirstAndNotSecond(client, enemy2, enemy1);
            }
        }

        private async Task AssertAttackedFirstAndNotSecond(HttpClient client, string path1, string path2)
        {
            var enemy2 = await GetUnitOfPath(client, path2);
            Assert.Equal(expectedHealthPoints[enemy2["type"].Value<string>()], enemy2["health"].Value<int>());

            using var response = await client.GetAsync(path1);
            if (response.IsSuccessStatusCode)
            {
                var enemy1 = await response.Content.ReadAsAsync<JObject>();
                Assert.NotEqual(expectedHealthPoints[enemy1["type"].Value<string>()], enemy1["health"].Value<int>());
            }
        }

        private async Task<(JObject, JObject)> GetDuelStatus(HttpClient client, (string, string) units)
        {
            return (await GetUnitOfPath(client, units.Item1), 
                await GetUnitOfPath(client, units.Item2));
        }

        private async Task<JObject> GetUnitOfPath(HttpClient client, string path)
        {
            using var response = await client.GetAsync(path);
            await response.AssertSuccessStatusCode();
            return await response.Content.ReadAsAsync<JObject>();
        }

        private async Task AssertPathNotFound(HttpClient client, string path)
        {
            using var response = await client.GetAsync(path);
            await response.AssertStatusEquals(HttpStatusCode.NotFound);
        }

        private async Task<(string, string)> CreateDuel(HttpClient client, string unitType1, string unitType2)
        {
            var system = await GetRandomSystemOtherThan(null);

            var unitPath1 = await CreateUserWithUnit(client, unitType1, system);
            var unitPath2 = await CreateUserWithUnit(client, unitType2, system);
            return (unitPath1, unitPath2);
        }

        private async Task<string> CreateUserWithUnit(HttpClient client, string unitType, string system)
        {
            var userPath = await CreateNewUserPath();
            return await CreateUnit(client, unitType, system, userPath);
        }

        private static async Task<string> CreateUnit(HttpClient client, string unitType, string system, string userPath)
        {
            var unitId = Guid.NewGuid().ToString();

            var response = await client.PutAsJsonAsync($"{userPath}/units/{unitId}", new
            {
                id = unitId,
                Type = unitType,
                system
            });

            await response.AssertSuccessStatusCode();
            var unit = await response.Content.ReadAsAsync<JObject>();
            return $"{userPath}/units/{unit["id"]}";
        }
    }
}
