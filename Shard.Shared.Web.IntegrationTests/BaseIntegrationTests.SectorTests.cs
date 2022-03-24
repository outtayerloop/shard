using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Shard.Shared.Web.IntegrationTests
{
    public partial class BaseIntegrationTests<TEntryPoint, TWebApplicationFactory>
    {
        
        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "1")]
        public async Task CanReadSystems()
        {
            using var client = factory.CreateClient();
            using var response = await client.GetAsync("systems");
            await response.AssertSuccessStatusCode();

            var array = await response.Content.ReadAsAsync<JArray>();
            Assert.NotEmpty(array);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "1")]
        public async Task SystemsHaveNames()
        {
            using var client = factory.CreateClient();
            using var response = await client.GetAsync("systems");
            await response.AssertSuccessStatusCode();

            var array = await response.Content.ReadAsAsync<JArray>();

            Assert.NotNull(array[0]["name"]);
            Assert.Equal(JTokenType.String, array[0]["name"].Type);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "1")]
        public async Task SystemsHavePlanets()
        {
            using var client = factory.CreateClient();
            using var response = await client.GetAsync("systems");
            await response.AssertSuccessStatusCode();

            var array = await response.Content.ReadAsAsync<JArray>();

            Assert.NotNull(array[0]["planets"]);
            Assert.Equal(JTokenType.Array, array[0]["planets"].Type);
            Assert.NotEmpty(array[0]["planets"]);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "1")]
        public async Task PlanetsHaveNames()
        {
            using var client = factory.CreateClient();
            using var response = await client.GetAsync("systems");
            await response.AssertSuccessStatusCode();

            var array = await response.Content.ReadAsAsync<JArray>();

            Assert.NotNull(array[0]["planets"][0]["name"]);
            Assert.Equal(JTokenType.String, array[0]["planets"][0]["name"].Type);

            var names = array.SelectTokens("$[*].planets[*].name").Select(token => token.Value<string>());
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "1")]
        public async Task PlanetsHaveSizes()
        {
            using var client = factory.CreateClient();
            using var response = await client.GetAsync("systems");
            await response.AssertSuccessStatusCode();

            var array = await response.Content.ReadAsAsync<JArray>();

            Assert.NotNull(array.SelectToken("[0].planets[0].size"));
            Assert.Equal(JTokenType.Integer, array.SelectToken("[0].planets[0].size").Type);
            Assert.Equal(JTokenType.Integer, array.SelectToken("[0].planets[0].size").Type);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "2")]
        public async Task PlanetsDoNotHaveResources()
        {
            using var client = factory.CreateClient();
            using var response = await client.GetAsync("systems");
            await response.AssertSuccessStatusCode();

            var array = await response.Content.ReadAsAsync<JArray>();
            var allPlanets = array.SelectTokens("[*].planets[*]").Cast<IDictionary<string, JToken>>();
            var allProperties = allPlanets.SelectMany(planet => planet.Keys).Distinct();
            Assert.DoesNotContain("resource", string.Join(",", allProperties));
        }

        public async Task<JToken> GetFirstSystem()
        {
            using var client = factory.CreateClient();
            using var systemsResponse = await client.GetAsync("systems");
            await systemsResponse.AssertSuccessStatusCode();

            var systems = await systemsResponse.Content.ReadAsAsync<JArray>();
            var system = systems.FirstOrDefault();
            Assert.NotNull(system);

            return system;
        }
        
        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "1")]
        public async Task CanFetchOneSystem()
        {
            var system = await GetFirstSystem();

            using var client = factory.CreateClient();
            using var response = await client.GetAsync("systems/" + system["name"].Value<string>());
            await response.AssertSuccessStatusCode();

            Assert.Equal(system.ToString(), (await response.Content.ReadAsAsync<JToken>()).ToString());
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "1")]
        public async Task CanFetchPlanetsOfOneSystem()
        {
            var system = await GetFirstSystem();

            using var client = factory.CreateClient();
            using var response = await client.GetAsync("systems/" + system["name"].Value<string>() + "/planets");
            await response.AssertSuccessStatusCode();

            Assert.Equal(system["planets"].ToString(), (await response.Content.ReadAsAsync<JToken>()).ToString());
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "1")]
        public async Task CanFetchOnePlanet()
        {
            var system = await GetFirstSystem();
            var planet = system["planets"][0];

            using var client = factory.CreateClient();
            using var response = await client.GetAsync(
                "systems/" + system["name"].Value<string>() + "/planets/" + planet["name"].Value<string>());
            await response.AssertSuccessStatusCode();

            Assert.Equal(planet.ToString(), (await response.Content.ReadAsAsync<JToken>()).ToString());
        }

        [Fact]
        [Trait("grading", "true")]
        public async Task NonExistingSectorReturns404()
        {
            using var client = factory.CreateClient();
            using var response = await client.GetAsync("systems");
            await response.AssertSuccessStatusCode();

            var array = await response.Content.ReadAsAsync<JArray>();

            Assert.NotNull(array.SelectToken("[0].planets[0].size"));
            Assert.Equal(JTokenType.Integer, array.SelectToken("[0].planets[0].size").Type);
            Assert.Equal(JTokenType.Integer, array.SelectToken("[0].planets[0].size").Type);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "4")]
        public async Task SystemIsFollowingTestSpecifications()
        {
            var expectedJson = GetExpectedJson("expectedTestSector.json")?.Replace("\r", string.Empty);

            using var client = factory.CreateClient();
            using var response = await client.GetAsync("systems");
            await response.AssertSuccessStatusCode();

            var array = await response.Content.ReadAsAsync<JArray>();
            Assert.Equal(expectedJson, array.ToString(Formatting.Indented)?.Replace("\r", string.Empty));
        } 
 
        private static string GetExpectedJson(string fileName)
        {

            // We assume test files are under the current assembly 
            // AND the same namespace (or a child one)
            var sibblingType = typeof(BaseIntegrationTests<TEntryPoint, TWebApplicationFactory>);
            var owningAssembly = sibblingType.Assembly;
            var baseNameSpace = sibblingType.Namespace;

            using (var stream = owningAssembly.GetManifestResourceStream(baseNameSpace + "." + fileName))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
