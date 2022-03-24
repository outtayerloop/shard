using System.Threading.Tasks;

namespace Shard.Shared.Web.IntegrationTests.Clock.TaskTracking
{
    // From https://github.com/xunit/xunit/blob/master/src/xunit.execution/Sdk/Utility/AsyncManualResetEvent.cs
    class AsyncManualResetEvent
    {
        volatile TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

        public AsyncManualResetEvent(bool signaled = false)
        {
            if (signaled)
                taskCompletionSource.TrySetResult(true);
        }

        public bool IsSet
        {
            get { return taskCompletionSource.Task.IsCompleted; }
        }

        public Task WaitAsync()
        {
            return taskCompletionSource.Task;
        }

        public void Set()
        {
            taskCompletionSource.TrySetResult(true);
        }

        public void Reset()
        {
            if (IsSet)
                taskCompletionSource = new TaskCompletionSource<bool>();
        }
    }
}
