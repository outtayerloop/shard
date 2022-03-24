using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Shard.Shared.Web.IntegrationTests
{
    public class FakeHttpHandler : DelegatingHandler
    {
        public class Entry
        {
            public Entry(
                HttpMethod expectedMethod, string expectedUri, 
                HttpStatusCode responseCode, HttpContent response)
            {
                ExpectedMethod = expectedMethod;
                ExpectedUri = expectedUri.Trim('/');
                ResponseCode = responseCode;
                Response = response;
            }

            public HttpMethod ExpectedMethod { get; }
            public string ExpectedUri { get; }

            public HttpStatusCode ResponseCode { get; }
            public HttpContent Response { get; }

            public HttpContent ActualRequestContent { get; set; }

            public bool Matches(HttpRequestMessage request)
            {
                return ExpectedMethod == request.Method
                    && ExpectedUri == request.RequestUri.ToString().Trim('/');
            }
        }

        private readonly List<Entry> entries = new List<Entry>();

        public Entry AddHandler(HttpMethod method, string uri, HttpStatusCode responseCode, HttpContent response)
        {
            var entry = new Entry(method, uri, responseCode, response);
            entries.Add(entry);
            return entry;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var entry = entries.SingleOrDefault(entry => entry.Matches(request))
                ?? throw new InvalidOperationException($"{request} was not expected");

            // When request will be disposed, so will its content. We need to clone it.
            entry.ActualRequestContent = await CloneContent(request.Content);

            var response = request.CreateResponse(entry.ResponseCode);
            response.Content = entry.Response;
            return response;
        }

        private static async Task<HttpContent> CloneContent(HttpContent originalContent)
        {
            var cloneContent = new StringContent((await originalContent.ReadAsStringAsync()).ToLower());

            if (originalContent.Headers != null)
            {
                cloneContent.Headers.Clear();
                foreach (var headerPair in originalContent.Headers)
                {
                    cloneContent.Headers.Add(headerPair.Key, headerPair.Value);
                }
            }
            return cloneContent;
        }
    }
}
