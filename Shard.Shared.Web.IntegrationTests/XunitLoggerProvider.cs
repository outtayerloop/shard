﻿using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Shard.Shared.Web.IntegrationTests
{
    public class XunitLoggerProvider : ILoggerProvider
    {
        private readonly ITestOutputHelper testOutputHelper;

        public XunitLoggerProvider(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        public ILogger CreateLogger(string categoryName)
            => new XunitLogger(testOutputHelper, categoryName);

        public void Dispose()
        { }
    }
}
