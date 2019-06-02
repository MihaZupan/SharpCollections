#if NETCORE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SharpCollections.Generic
{
    public class WorkSchedulerTests
    {
        private class MockTaskScheduler : TaskScheduler
        {
            public readonly List<Task> Tasks = new List<Task>();

            protected override IEnumerable<Task> GetScheduledTasks()
            {
                return Tasks;
            }

            protected override void QueueTask(Task task)
            {
                Tasks.Add(task);
            }

            protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
            {
                return false;
            }
        }

        [Fact]
        public async Task ExecutesWork()
        {
            int sum = 0;

            WorkScheduler<int> scheduler = new WorkScheduler<int>(work => {
                Interlocked.Add(ref sum, work);
                return Task.CompletedTask;
            });

            scheduler.Enqueue(1, 0);
            scheduler.Enqueue(2, 1);
            scheduler.Enqueue(3, 2);
            scheduler.Enqueue(4, 3);

            Assert.Equal(0, scheduler.PendingWorkItems);

            var workItems = await scheduler.StopAndWaitForCompletionAsync();
            Assert.Empty(workItems);

            Assert.Equal(10, sum);
        }
    }
}
#endif