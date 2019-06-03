#if NETCORE
using System;
using System.Collections.Generic;
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

        [Fact]
        public async Task ExecutesWorkInParallel()
        {
            long sum = 0;

            ManualResetEvent mre = new ManualResetEvent(false);

            WorkScheduler<int> scheduler = new WorkScheduler<int>(work => {
                Interlocked.Add(ref sum, work);
                mre.WaitOne();
                Interlocked.Add(ref sum, -work);
                return Task.CompletedTask;
            });

            scheduler.Enqueue(1, 0);
            scheduler.Enqueue(2, 1);
            scheduler.Enqueue(3, 2);
            scheduler.Enqueue(4, 3);
            scheduler.Enqueue(5, 4);

            Assert.Equal(0, scheduler.PendingWorkItems);

            SpinWait.SpinUntil(() => Interlocked.Read(ref sum) == 15);

            Assert.Equal(15, sum);

            mre.Set();

            var workItems = await scheduler.StopAndWaitForCompletionAsync();
            Assert.Empty(workItems);

            Assert.Equal(0, sum);
        }

        [Fact]
        public async Task ExecutesOneItemFromBucketAtATime()
        {
            long sum = 0;

            ManualResetEvent mre = new ManualResetEvent(false);

            WorkScheduler<int> scheduler = new WorkScheduler<int>(work => {
                Interlocked.Add(ref sum, work);
                mre.WaitOne();
                return Task.CompletedTask;
            });

            scheduler.Enqueue(1, 0);
            scheduler.Enqueue(2, 1);
            scheduler.Enqueue(3, 1);

            Assert.Equal(1, scheduler.PendingWorkItems);

            SpinWait.SpinUntil(() => Interlocked.Read(ref sum) == 3);
            Assert.Equal(3, sum);

            mre.Set();

            SpinWait.SpinUntil(() => Interlocked.Read(ref sum) == 6);
            Assert.Equal(6, sum);

            Assert.Equal(0, scheduler.PendingWorkItems);

            var workItems = await scheduler.StopAndWaitForCompletionAsync();
            Assert.Empty(workItems);

            Assert.Equal(6, sum);
        }

        [Fact]
        public async Task RetrievesNonScheduledWork()
        {
            long sum = 0;

            ManualResetEvent mre = new ManualResetEvent(false);

            WorkScheduler<int> scheduler = new WorkScheduler<int>(work => {
                Interlocked.Add(ref sum, work);
                mre.WaitOne();
                return Task.CompletedTask;
            });

            scheduler.Enqueue(1, 0);
            scheduler.Enqueue(2, 0);
            scheduler.Enqueue(3, 1);
            scheduler.Enqueue(4, 1);

            Assert.Equal(2, scheduler.PendingWorkItems);

            SpinWait.SpinUntil(() => Interlocked.Read(ref sum) == 4);
            Assert.Equal(4, sum);

            _ = Task.Run(async () => { await Task.Delay(100); mre.Set(); });

            var workItems = await scheduler.StopAndWaitForCompletionAsync();

            Assert.Equal(0, scheduler.PendingWorkItems);

            Assert.Equal(4, sum);

            Assert.Equal(new[] { 2, 4 }, workItems);
        }

        [Fact]
        public async Task HonorsMaxDegreeOfParallelism()
        {
            long sum = 0;

            ManualResetEvent mre = new ManualResetEvent(false);

            WorkScheduler<int> scheduler = new WorkScheduler<int>(work => {
                Interlocked.Add(ref sum, work);
                mre.WaitOne();
                return Task.CompletedTask;
            }, maxDegreeOfParallelism: 2);

            Assert.Equal(2, scheduler.MaxDegreeOfParallelism);

            scheduler.Enqueue(1, 0);
            scheduler.Enqueue(2, 0);
            scheduler.Enqueue(3, 1);
            scheduler.Enqueue(4, 2);
            scheduler.Enqueue(5, 1);
            scheduler.Enqueue(6, 2);

            Assert.Equal(4, scheduler.PendingWorkItems);

            SpinWait.SpinUntil(() => Interlocked.Read(ref sum) == 4);
            Assert.Equal(4, sum);

            _ = Task.Run(async () => { await Task.Delay(100); mre.Set(); });

            var workItems = await scheduler.StopAndWaitForCompletionAsync();

            Assert.Equal(0, scheduler.PendingWorkItems);

            Assert.Equal(4, sum);

            Assert.Equal(new[] { 2, 4, 5, 6 }, workItems);
        }

        [Fact]
        public async Task HonorsUserSpecifiedPriority()
        {
            long value = 0;

            ManualResetEvent mre = new ManualResetEvent(false);

            WorkScheduler<int> scheduler = new WorkScheduler<int>(work => {
                Interlocked.Add(ref value, work);
                mre.WaitOne();
                mre.Reset();
                return Task.CompletedTask;
            }, maxDegreeOfParallelism: 1);

            scheduler.Enqueue(1, 0, bucketPriority: 1);
            scheduler.Enqueue(2, 0, bucketPriority: 1);
            scheduler.Enqueue(3, 1, bucketPriority: 3);
            scheduler.Enqueue(4, 2, bucketPriority: 2);
            scheduler.Enqueue(5, 1, bucketPriority: 3);
            scheduler.Enqueue(6, 2, bucketPriority: 2);
            // 1 goes first as it is the first one enqueued
            // then follow 3, 5, 4, 6, 2

            Assert.Equal(5, scheduler.PendingWorkItems);

            SpinWait.SpinUntil(() => Interlocked.Read(ref value) == 1);
            Assert.Equal(1, value);
            value = 0;
            Assert.Equal(5, scheduler.PendingWorkItems);

            mre.Set();

            SpinWait.SpinUntil(() => Interlocked.Read(ref value) == 3);
            Assert.Equal(3, value);
            value = 0;
            Assert.Equal(4, scheduler.PendingWorkItems);

            mre.Set();

            SpinWait.SpinUntil(() => Interlocked.Read(ref value) == 5);
            Assert.Equal(5, value);
            value = 0;
            Assert.Equal(3, scheduler.PendingWorkItems);

            mre.Set();

            SpinWait.SpinUntil(() => Interlocked.Read(ref value) == 4);
            Assert.Equal(4, value);
            value = 0;
            Assert.Equal(2, scheduler.PendingWorkItems);

            mre.Set();

            SpinWait.SpinUntil(() => Interlocked.Read(ref value) == 6);
            Assert.Equal(6, value);
            value = 0;
            Assert.Equal(1, scheduler.PendingWorkItems);

            mre.Set();

            SpinWait.SpinUntil(() => Interlocked.Read(ref value) == 2);
            Assert.Equal(2, value);
            value = 0;
            Assert.Equal(0, scheduler.PendingWorkItems);

            mre.Set();

            var workItems = await scheduler.StopAndWaitForCompletionAsync();
            Assert.Empty(workItems);

            Assert.Equal(0, scheduler.PendingWorkItems);
        }

        [Fact]
        public async Task RetrievesNonScheduledWorkInSortedOrder()
        {
            long sum = 0;

            ManualResetEvent mre = new ManualResetEvent(false);

            WorkScheduler<int> scheduler = new WorkScheduler<int>(work => {
                Interlocked.Add(ref sum, work);
                mre.WaitOne();
                return Task.CompletedTask;
            }, maxDegreeOfParallelism: 1);

            scheduler.Enqueue(1, 0, bucketPriority: 1);
            scheduler.Enqueue(2, 0);
            scheduler.Enqueue(3, 1, bucketPriority: 2);
            scheduler.Enqueue(4, 2);
            scheduler.Enqueue(5, 1, bucketPriority: 3);
            scheduler.Enqueue(6, 2);

            Assert.Equal(5, scheduler.PendingWorkItems);

            SpinWait.SpinUntil(() => Interlocked.Read(ref sum) == 1);
            Assert.Equal(1, sum);
            Assert.Equal(5, scheduler.PendingWorkItems);

            _ = Task.Run(async () => { await Task.Delay(100); mre.Set(); });

            var workItems = await scheduler.StopAndWaitForCompletionAsync();

            Assert.Equal(0, scheduler.PendingWorkItems);

            Assert.Equal(1, sum);

            Assert.Equal(new[] { 5, 3, 2, 4, 6 }, workItems);
        }

        [Fact]
        public async Task StopsIfNoWorkIsPending()
        {
            long sum = 0;

            WorkScheduler<int> scheduler = new WorkScheduler<int>(work => {
                Interlocked.Add(ref sum, work);
                return Task.CompletedTask;
            });

            var workItems = await scheduler.StopAndWaitForCompletionAsync();
            Assert.Empty(workItems);

            Assert.Equal(0, scheduler.PendingWorkItems);
        }

        [Fact]
        public async Task ProcessesMassiveWorkBuffer()
        {
            const int bucketSize = 4;
            var rng = new Random();

            void Shuffle<T>(T[] array)
            {
                int n = array.Length;
                while (n > 1)
                {
                    int k = rng.Next(n--);
                    T temp = array[n];
                    array[n] = array[k];
                    array[k] = temp;
                }
            }

            byte[] priorities = new byte[256];
            for (int i = 0; i < priorities.Length; i++) priorities[i] = (byte)i;
            Shuffle(priorities);

            byte[][] buckets = new byte[256][];
            for (int i = 0; i < buckets.Length; i++)
            {
                var bucket = buckets[i] = new byte[bucketSize];
                rng.NextBytes(bucket);
                for (int j = 0; j < bucket.Length; j++)
                    if (bucket[j] == 0) bucket[j] = (byte)rng.Next(1, 256);
            }

            long value = 0;
            ManualResetEvent mre = new ManualResetEvent(false);

            WorkScheduler<byte> scheduler = new WorkScheduler<byte>(work => {
                Interlocked.Add(ref value, work);
                mre.WaitOne();
                mre.Reset();
                return Task.CompletedTask;
            }, maxDegreeOfParallelism: 1);

            scheduler.Enqueue(1, -1, 0);
            SpinWait.SpinUntil(() => Interlocked.Read(ref value) == 1);
            value = 0;

            for (int i = 0; i < priorities.Length; i++)
            {
                byte priority = priorities[i];
                var bucket = buckets[priority];
                for (int j = 0; j < bucket.Length; j++)
                {
                    scheduler.Enqueue(work: bucket[j], bucket: i, bucketPriority: priority);
                }
            }

            int count = 256 * bucketSize;
            Assert.Equal(count, scheduler.PendingWorkItems);

            for (int i = 255; i >= 0; i--)
            {
                var bucket = buckets[i];
                for (int j = 0; j < bucket.Length; j++)
                {
                    int pending = scheduler.PendingWorkItems;
                    Assert.True(count == pending || count == pending - 1);
                    mre.Set();
                    SpinWait.SpinUntil(() => Interlocked.Read(ref value) == bucket[j]);
                    value = 0;
                    count--;
                    pending = scheduler.PendingWorkItems;
                    Assert.True(count == pending || count == pending - 1);
                }
            }

            Assert.Equal(0, scheduler.PendingWorkItems);

            for (int i = 0; i < priorities.Length; i++)
            {
                byte priority = priorities[i];
                var bucket = buckets[priority];
                for (int j = 0; j < bucket.Length; j++)
                {
                    scheduler.Enqueue(work: bucket[j], bucket: i, bucketPriority: priority);
                }
            }

            Assert.Equal(256 * bucketSize, scheduler.PendingWorkItems);

            _ = Task.Run(async () => { await Task.Delay(100); mre.Set(); });

            var workItems = await scheduler.StopAndWaitForCompletionAsync();

            Assert.Equal(0, scheduler.PendingWorkItems);
            Assert.Equal(256 * bucketSize, workItems.Length);

            count = 0;
            for (int i = 255; i >= 0; i--)
            {
                var bucket = buckets[i];
                for (int j = 0; j < bucket.Length; j++)
                {
                    Assert.Equal(workItems[count++], bucket[j]);
                }
            }
        }
    }
}
#endif