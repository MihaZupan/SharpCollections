// Copyright (c) Miha Zupan. All rights reserved.
// This file is licensed under the BSD-Clause 2 license. 
// See the license.txt file in the project root for more information.
using SharpCollections.Helpers;
using System;

namespace SharpCollections.Generic
{
    /// <summary>
    /// A simple generic Binary Heap implementation. Min-first order with default <see cref="IComparable{T}"/> behavior.
    /// </summary>
    /// <typeparam name="T">Type of heap element. Must implement <see cref="IComparable{T}"/>.</typeparam>
    public class BinaryHeap<T> where T: IComparable<T>
    {
        private T[] _heap;

        /// <summary>
        /// Amount of elements in the heap.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Amount of elements the heap can contain before resizing
        /// </summary>
        public int Capacity
        {
            get => _heap.Length - 1;
            set
            {
                if (value < Count || value == int.MaxValue)
                    throw new ArgumentOutOfRangeException(nameof(value), "Must be >= Count and < int.MaxValue");

                if (value != _heap.Length - 1)
                {
                    T[] newHeap = new T[1 + value];
                    Array.Copy(_heap, 1, newHeap, 1, Count);
                    _heap = newHeap;
                }
            }
        }

        /// <summary>
        /// => Count == 0;
        /// </summary>
        public bool IsEmpty => Count == 0;

        /// <summary>
        /// Constructs a new <see cref="BinaryHeap{T}"/> with the specified initial capacity.
        /// </summary>
        /// <param name="capacity">The initial capacity for the <see cref="BinaryHeap{T}"/>.</param>
        public BinaryHeap(int capacity = 0)
        {
            if (capacity < 0 || capacity == int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(capacity), "Must be >= 0 and < int.MaxValue");

            _heap = new T[1 + capacity];
        }

        /// <summary>
        /// Looks up the top element in the heap. Min with default <see cref="IComparable{T}"/> behavior. O(1).
        /// </summary>
        public T Top
        {
            get
            {
                if (Count == 0)
                    throw new InvalidOperationException("BinaryHeap is empty");

                return _heap[1];
            }
        }

        /// <summary>
        /// Retrieves and removes the top element in the heap. O(logN).
        /// </summary>
        /// <returns>The item previously at the top of the heap.</returns>
        public T Pop()
        {
            var top = Top;

            var heap = _heap;

            var tmp = heap[1] = heap[Count];

            heap[Count--] = default;

            if (Count == 0)
                return top;

            int pos = 1;
            int child = pos << 1;

            while (child <= Count)
            {
                if (child != Count && heap[child].CompareTo(heap[child + 1]) > 0)
                    child++;

                if (tmp.CompareTo(heap[child]) > 0)
                    heap[pos] = heap[child];
                else
                    break;

                pos = child;
                child <<= 1;
            }
            heap[pos] = tmp;

            return top;
        }

        /// <summary>
        /// Inserts an element into the heap. O(logN).
        /// </summary>
        /// <param name="item"></param>
        public void Push(T item)
        {
            if (item == null)
                ThrowHelper.ArgumentNullException(ExceptionArgument.item);

            if (Count == Capacity)
                Grow();

            var heap = _heap;

            int pos = ++Count;

            while (pos > 1)
            {
                int parent = pos >> 1;
                ref var parentNode = ref heap[parent];

                if (item.CompareTo(parentNode) >= 0)
                    break;

                heap[pos] = parentNode;
                pos = parent;
            }

            heap[pos] = item;
        }

        private void Grow()
        {
            if (_heap.Length == int.MaxValue)
                throw new InvalidOperationException("Reached maximum capacity");

            int newCapacity = Capacity * 2;

            if (newCapacity == 0)
            {
                newCapacity = 4;
            }
            else if ((uint)newCapacity >= int.MaxValue)
            {
                newCapacity = int.MaxValue - 1;
            }

            Capacity = newCapacity;
        }

        /// <summary>
        /// Removes all elements in the heap.
        /// </summary>
        public void Clear()
        {
            Array.Clear(_heap, 1, Count);
            Count = 0;
        }
    }
}
