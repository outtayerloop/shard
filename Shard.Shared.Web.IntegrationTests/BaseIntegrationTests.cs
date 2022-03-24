using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Shard.Shared.Core;
using Shard.Shared.Web.IntegrationTests.Clock;
using System;
using Xunit;
using Xunit.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace Shard.Shared.Web.IntegrationTests
{
    public abstract partial class BaseIntegrationTests<TEntryPoint, TWebApplicationFactory>
        : IClassFixture<TWebApplicationFactory>, IHttpMessageHandlerBuilderFilter
        where TEntryPoint : class
        where TWebApplicationFactory: WebApplicationFactory<TEntryPoint>
    {
        private readonly WebApplicationFactory<TEntryPoint> factory;
        private readonly FakeClock fakeClock = new FakeClock();
        private readonly FakeHttpHandler httpHandler = new FakeHttpHandler();

        public BaseIntegrationTests(TWebApplicationFactory factory, ITestOutputHelper testOutputHelper = null)
        {
            this.factory = factory
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureAppConfiguration(RemoveAllReloadOnChange);

                    if (testOutputHelper != null)
                    {
                        builder.ConfigureLogging(
                            logging => logging.AddProvider(new XunitLoggerProvider(testOutputHelper)));
                    }

                    builder.ConfigureAppConfiguration(config =>
                    {
                        config.AddInMemoryCollection(new Dictionary<string, string>()
                        {
                            { "Wormholes:fake-remote:baseUri", "http://10.0.0.42" },
                            { "Wormholes:fake-remote:system", "80ad7191-ef3c-14f0-7be8-e875dad4cfa6" },
                            { "Wormholes:fake-remote:user", "server1" },
                            { "Wormholes:fake-remote:sharedPassword", "caramba" },
                        });
                    });

                    builder.ConfigureTestServices(services =>
                    {
                        services.AddSingleton<IClock>(fakeClock);
                        services.AddSingleton<IStartupFilter>(fakeClock);
                        services.Configure<MapGeneratorOptions>(options =>
                        {
                            options.Seed = "Test application";
                        });
                        services.AddSingleton<IHttpMessageHandlerBuilderFilter>(this);
                    });
                });
        }

        private void RemoveAllReloadOnChange(WebHostBuilderContext context, IConfigurationBuilder configuration)
        {
            foreach (var source in configuration.Sources.OfType<FileConfigurationSource>())
                source.ReloadOnChange = false;
        }


        public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next)
        {
            return builder =>
            {
                builder.AdditionalHandlers.Add(httpHandler);
                next(builder);
            };
        }
    }
}
