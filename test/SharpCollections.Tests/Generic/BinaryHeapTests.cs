using System;
using Xunit;

namespace SharpCollections.Generic
{
    public class BinaryHeapTests
    {
        private readonly struct TestType : IComparable<TestType>
        {
            public readonly int Priority;

            public TestType(int priority)
            {
                Priority = priority;
            }

            public int CompareTo(TestType other)
            {
                return Priority.CompareTo(other.Priority);
            }
        }

        [Fact]
        public void ReturnsItemsInCorrectOrder()
        {
            BinaryHeap<TestType> heap = new BinaryHeap<TestType>();

            heap.Push(new TestType(123));
            heap.Push(new TestType(124));
            heap.Push(new TestType(120));
            heap.Push(new TestType(95));

            Assert.Equal(95, heap.Pop().Priority);

            heap.Push(new TestType(-1));

            Assert.Equal(-1, heap.Pop().Priority);

            Assert.Equal(120, heap.Pop().Priority);
            Assert.Equal(123, heap.Pop().Priority);
            Assert.Equal(124, heap.Pop().Priority);
        }

        [Fact]
        public void ReportsCorrectItemCount()
        {
            BinaryHeap<TestType> heap = new BinaryHeap<TestType>();

            Assert.Equal(0, heap.Count);

            heap.Push(new TestType(123));
            heap.Push(new TestType(124));

            Assert.Equal(2, heap.Count);

            Assert.Equal(123, heap.Pop().Priority);

            Assert.Equal(1, heap.Count);

            Assert.Equal(124, heap.Pop().Priority);

            Assert.Equal(0, heap.Count);

            heap.Push(new TestType(-1));

            Assert.Equal(1, heap.Count);

            Assert.Equal(-1, heap.Pop().Priority);

            Assert.Equal(0, heap.Count);
        }

        [Fact]
        public void ReportsCorrectCapacity()
        {
            BinaryHeap<TestType> heap = new BinaryHeap<TestType>(7);

            Assert.Equal(0, heap.Count);
            Assert.Equal(7, heap.Capacity);

            heap.Push(new TestType(1));
            heap.Push(new TestType(2));
            heap.Push(new TestType(3));
            heap.Push(new TestType(4));
            heap.Push(new TestType(5));
            heap.Push(new TestType(6));
            heap.Push(new TestType(7));

            Assert.Equal(7, heap.Capacity);

            heap.Push(new TestType(8));

            Assert.True(heap.Capacity > 7);
        }

        [Fact]
        public void ResizesByDoubling()
        {
            BinaryHeap<TestType> heap = new BinaryHeap<TestType>();

            Assert.Equal(0, heap.Count);
            Assert.Equal(3, heap.Capacity);

            heap.Push(new TestType(1));
            heap.Push(new TestType(2));
            heap.Push(new TestType(3));

            Assert.Equal(3, heap.Capacity);

            heap.Push(new TestType(4));

            Assert.Equal(6, heap.Capacity);

            heap.Push(new TestType(5));
            heap.Push(new TestType(6));

            Assert.Equal(6, heap.Capacity);

            heap.Push(new TestType(7));

            Assert.Equal(12, heap.Capacity);
        }

        [Fact]
        public void ClearsItems()
        {
            BinaryHeap<TestType> heap = new BinaryHeap<TestType>();

            Assert.Equal(0, heap.Count);

            heap.Push(new TestType(1));
            heap.Push(new TestType(2));
            heap.Push(new TestType(3));

            Assert.Equal(3, heap.Count);

            heap.Clear();

            Assert.Equal(0, heap.Count);
        }

        [Fact]
        public void ThrowsOnNullPush()
        {
            BinaryHeap<string> heap = new BinaryHeap<string>();

            Assert.Throws<ArgumentNullException>(() => { heap.Push(default); });

            BinaryHeap<int> heapOfValueTypes = new BinaryHeap<int>();

            // Does not throw on default value
            heapOfValueTypes.Push(default);
        }

        [Fact]
        public void ThrowsOnInvalidCapacitySizes()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => { new BinaryHeap<string>(-1); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { new BinaryHeap<string>(int.MaxValue); });

            BinaryHeap<string> heap = new BinaryHeap<string>(10);

            Assert.Equal(0, heap.Count);
            Assert.Equal(10, heap.Capacity);

            // Can resize to fit
            heap.Capacity = 0;

            Assert.Equal(0, heap.Capacity);

            Assert.Throws<ArgumentOutOfRangeException>(() => { heap.Capacity = -1; });
            Assert.Throws<ArgumentOutOfRangeException>(() => { heap.Capacity = int.MaxValue; });

            heap.Push("foo");

            Assert.Throws<ArgumentOutOfRangeException>(() => { heap.Capacity = 0; });

            heap.Capacity = 1;

            heap.Pop();

            heap.Capacity = 0;
        }

        [Fact]
        public void ThrowsOnPopWhenEmpty()
        {
            BinaryHeap<int> heap = new BinaryHeap<int>();

            Assert.Throws<InvalidOperationException>(() => { _ = heap.Top; });
            Assert.Throws<InvalidOperationException>(() => { _ = heap.Pop(); });
        }
    }
}
