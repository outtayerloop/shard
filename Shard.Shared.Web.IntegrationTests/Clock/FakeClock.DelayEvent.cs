using Shard.Shared.Web.IntegrationTests.Clock.TaskTracking;
using System;
using System.Threading;
using Xunit.Sdk;

namespace Shard.Shared.Web.IntegrationTests.Clock
{
    public partial class FakeClock
    {
        private class DelayEvent : BaseDelayEvent, IEvent
        {
            public DateTime TriggerTime { get; }

            public DelayEvent(DateTime triggerTime, CancellationToken cancellationToken, AsyncTrackingSyncContext asyncTestSyncContext)
                : base(cancellationToken, asyncTestSyncContext)
            {
                TriggerTime = triggerTime;
            }
        }
    }
}