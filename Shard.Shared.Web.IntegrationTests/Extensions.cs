using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

namespace Shard.Shared.Web.IntegrationTests
{
    public static class Extensions
    {
        public static async Task AssertSuccessStatusCode(this HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                await ThrowsWithContent(response, 
                    $"Expected valid status code, got {(int)response.StatusCode} {response.StatusCode}.");
            }
        }

        public static async Task AssertStatusEquals(this HttpResponseMessage response, HttpStatusCode expected)
        {
            if (response.StatusCode != expected)
            {
                await ThrowsWithContent(response, 
                    $"Expected {(int)expected} {expected} status code, got {(int)response.StatusCode} {response.StatusCode}.");
            }
        }

        public static async Task AssertStatusCodeAmong(this HttpResponseMessage response, params HttpStatusCode[] expected)
        {
            if (!expected.Contains(response.StatusCode))
            {
                await ThrowsWithContent(response,
                    $"Expected status code among {string.Join(", ", expected)}; got {(int)response.StatusCode} {response.StatusCode}.");
            }
        }

        private static async Task ThrowsWithContent(HttpResponseMessage response, string message)
        {
            string contentMessage = await GetMessageWontent(response);
            throw new XunitException(message + contentMessage);
        }

        private static async Task<string> GetMessageWontent(HttpResponseMessage response)
        {
            return response.Content != null
                ? string.Concat("Body:", Environment.NewLine, await response.Content.ReadAsStringAsync())
                : "No body";
        }
    }
}
