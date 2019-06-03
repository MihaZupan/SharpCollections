// Copyright (c) Miha Zupan. All rights reserved.
// This file is licensed under the BSD-Clause 2 license. 
// See the license.txt file in the project root for more information.
#if NETCORE
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SharpCollections.Generic
{
    /// <summary>
    /// A class that helps schedule work on a <see cref="TaskScheduler"/>.
    /// <para>Work is processed in order of insertion/priority, possibly in parallel.</para>
    /// <para>Ensures that only one work item from each bucket is processed at the same time.</para>
    /// </summary>
    /// <typeparam name="T">Type of work item - state necesarry to perform the work routine.</typeparam>
    public class WorkScheduler<T>
    {
        private readonly struct Node : IComparable<Node>
        {
            public readonly T Value;
            public readonly ulong Priority;
            public readonly long Bucket;

            public Node(T value, ulong priority, long bucket)
            {
                Value = value;
                Priority = priority;
                Bucket = bucket;
            }

            public int CompareTo(Node other)
            {
                // DONE IN REVERSE ORDER to sort items in descending order
                return other.Priority.CompareTo(Priority);
            }
        }

        private readonly TaskScheduler _taskScheduler;
        private readonly Func<T, Task> _workRoutine;
        private readonly Dictionary<long, Queue<Node>> _buckets;
        private int _activeWorkers;
        private long _workCounter;
        private int _pendingWorkItems;

        // Binary heap of work items - holds at most one item from each bucket at a time
        // greater<Node> order is used - first node is max, children are in descending order
        private Node[] _workHeap;
        private int _workHeapCount;

        private TaskCompletionSource<bool> _completionSource;

        /// <summary>
        /// Number of work items that have not yet been scheduled
        /// </summary>
        public int PendingWorkItems => _pendingWorkItems;

        /// <summary>
        /// Indicates that <see cref="StopAndWaitForCompletionAsync"/> has been called.
        /// <para>It can not be reverted - create a new instance of <see cref="WorkScheduler{T}"/>.</para>
        /// </summary>
        public bool IsStopped => !(_completionSource is null);

        /// <summary>
        /// Maximum amount of work items allowed to be scheduled on the <see cref="TaskScheduler"/>.
        /// </summary>
        public readonly int MaxDegreeOfParallelism;

        /// <summary>
        /// Creates a work scheduler with the specified work routine and degree of parallelism. <see cref="TaskScheduler.Default"/> is used.
        /// </summary>
        /// <param name="workRoutine">The asynchronous work routine to be scheduled on the <see cref="TaskScheduler"/> for each work item.</param>
        /// <param name="maxDegreeOfParallelism">Maximum amount of work items allowed to be scheduled on the <see cref="TaskScheduler"/> - defaults to <see cref="int.MaxValue"/>.</param>
        public WorkScheduler(Func<T, Task> workRoutine, int maxDegreeOfParallelism = 0)
            : this(workRoutine, maxDegreeOfParallelism, TaskScheduler.Default)
        { }

        /// <summary>
        /// Creates a work scheduler with the specified work routine and degree of parallelism.
        /// </summary>
        /// <param name="workRoutine">The asynchronous work routine to be scheduled on the <see cref="TaskScheduler"/> for each work item.</param>
        /// <param name="maxDegreeOfParallelism">Maximum amount of work items allowed to be scheduled on the <see cref="TaskScheduler"/> - defaults to <see cref="int.MaxValue"/>.</param>
        /// <param name="taskScheduler">The <see cref="TaskScheduler"/> to schedule work on.</param>
        public WorkScheduler(Func<T, Task> workRoutine, int maxDegreeOfParallelism, TaskScheduler taskScheduler)
        {
            _workRoutine = workRoutine ?? throw new ArgumentNullException(nameof(workRoutine));
            _taskScheduler = taskScheduler ?? throw new ArgumentNullException(nameof(_taskScheduler));

            MaxDegreeOfParallelism = maxDegreeOfParallelism <= 0 ? int.MaxValue : maxDegreeOfParallelism;

            _buckets = new Dictionary<long, Queue<Node>>(8);
            _workHeap = new Node[8];
            _workCounter = 1L << 56;
        }

        /// <summary>
        /// Enqueues a work item for processing on the <see cref="TaskScheduler"/>.
        /// <para>Only one work item from each bucket is processed at the same time.</para>
        /// </summary>
        /// <param name="work">Work item to be executed on the <see cref="TaskScheduler"/>.</param>
        /// <param name="bucket">Bucket that this work item belongs to. Only one work item from each bucket is processed at the same time.</param>
        /// <param name="priority">Scheduling priority that takes precedence over enqueueing time.</param>
        public void Enqueue(T work, long bucket, byte priority = 0)
        {
            ulong actualPriority = (ulong)priority << 56 | (ulong)Interlocked.Decrement(ref _workCounter);

            var workItem = new Node(work, actualPriority, bucket);

            lock (_buckets)
            {
                Interlocked.Increment(ref _pendingWorkItems);

                if (_buckets.TryGetValue(bucket, out Queue<Node> queue) || IsStopped)
                {
                    if (queue is null)
                    {
                        queue = new Queue<Node>(4);
                        _buckets[bucket] = queue;
                    }
                    queue.Enqueue(workItem);
                    return;
                }

                _buckets.Add(bucket, null);
            }

            lock (_workHeap)
            {
                if (_activeWorkers < MaxDegreeOfParallelism)
                {
                    _activeWorkers++;
                }
                else
                {
                    EnqueueToWorkHeap(workItem);
                    return;
                }
            }
            ScheduleWorkItemOnTaskScheduler(workItem);
        }

        /// <summary>
        /// Stops work from being scheduled on the <see cref="TaskScheduler"/> and waits for work that is already processing to complete.
        /// <para>Returns all non-scheduled work items, sorted by priority/insertion order.</para>
        /// <para>Work that is enqueued after calling this method is not guaranteed to be included in the return value,
        /// but subsequent calls to this method are allowed and will return any newly-enqueued work items.</para>
        /// <para>Scheduling will NOT be resumed after this method returns.</para>
        /// </summary>
        /// <returns>All non-scheduled work items, sorted based on priority</returns>
        public async Task<T[]> StopAndWaitForCompletionAsync()
        {
            lock (_buckets)
            {
                lock (_workHeap)
                {
                    _completionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                    if (_activeWorkers == 0)
                        _completionSource.SetResult(false);
                }
            }

            await _completionSource.Task.ConfigureAwait(false);

            Node[] nodes;
            int i;
            lock (_buckets)
            {
                lock (_workHeap)
                {
                    nodes = new Node[_pendingWorkItems];
                    _workHeap.AsSpan(1, _workHeapCount).CopyTo(nodes);
                    i = _workHeapCount;
                    foreach (var updateGroup in _buckets.Values)
                    {
                        if (updateGroup is null)
                            continue;

                        foreach (var node in updateGroup)
                            nodes[i++] = node;
                    }
                    _workHeap.AsSpan(1, _workHeapCount).Clear();
                    _workHeapCount = 0;
                    _buckets.Clear();
                }
            }

            Interlocked.Add(ref _pendingWorkItems, -i);

            Array.Sort(nodes, 0, i);

            T[] work = new T[i];
            for (i = 0; i < work.Length; i++)
                work[i] = nodes[i].Value;

            return work;
        }

        private void ScheduleWorkItemOnTaskScheduler(Node work)
        {
            Interlocked.Decrement(ref _pendingWorkItems);

            Task.Factory.StartNew(async () =>
            {
                bool workPending = true;
                while (workPending)
                {
                    workPending = false;

                    try
                    {
                        await _workRoutine(work.Value).ConfigureAwait(false);
                    }
                    finally
                    {
                        lock (_buckets)
                        {
                            var queue = _buckets[work.Bucket];
                            if (queue is null || queue.Count == 0)
                            {
                                _buckets.Remove(work.Bucket);
                            }
                            else
                            {
                                work = queue.Dequeue();
                                lock (_workHeap)
                                {
                                    EnqueueToWorkHeap(work);
                                }
                            }
                        }

                        lock (_workHeap)
                        {
                            if (IsStopped)
                            {
                                _activeWorkers--;
                                if (_activeWorkers == 0)
                                {
                                    _completionSource.SetResult(true);
                                }
                            }
                            else if (_workHeapCount == 0)
                            {
                                _activeWorkers--;
                            }
                            else
                            {
                                work = DequeueFromWorkHeap();
                                workPending = true;
                                Interlocked.Decrement(ref _pendingWorkItems);
                            }
                        }
                    }
                }
            }, default, TaskCreationOptions.DenyChildAttach, _taskScheduler);
        }

        private void EnqueueToWorkHeap(Node workNode)
        {
            Debug.Assert(Monitor.IsEntered(_workHeap));

            if (_workHeapCount == _workHeap.Length - 1)
                Array.Resize(ref _workHeap, _workHeap.Length * 2);

            int pos = ++_workHeapCount;
            var priority = workNode.Priority;

            while (pos > 1)
            {
                int parent = pos >> 1;
                ref var parentNode = ref _workHeap[parent];

                if (priority <= parentNode.Priority)
                    break;

                _workHeap[pos] = parentNode;
                pos = parent;
            }

            _workHeap[pos] = workNode;
        }

        private Node DequeueFromWorkHeap()
        {
            Debug.Assert(Monitor.IsEntered(_workHeap));

            var max = _workHeap[1];

            var tmp = _workHeap[1] = _workHeap[_workHeapCount--];

            ulong priority = tmp.Priority;
            int pos = 1;
            int child = pos << 1;

            while (child <= _workHeapCount)
            {
                if (child != _workHeapCount && _workHeap[child].Priority < _workHeap[child + 1].Priority)
                    child++;

                if (priority < _workHeap[child].Priority)
                    _workHeap[pos] = _workHeap[child];
                else
                    break;

                pos = child;
                child <<= 1;
            }
            _workHeap[pos] = tmp;

            return max;
        }
    }
}
#endif