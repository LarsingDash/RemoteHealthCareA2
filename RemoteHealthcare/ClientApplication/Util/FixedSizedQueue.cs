using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ClientApplication.Util;

public sealed class FixedSizedQueue<T> : Queue<T>
{
    private int FixedCapacity { get; }
    public FixedSizedQueue(int fixedCapacity)
    {
        this.FixedCapacity = fixedCapacity;
    }

    /// <summary>
    /// If the total number of item exceed the capacity, the oldest ones automatically dequeues.
    /// </summary>
    /// <returns>The dequeued value, if any.</returns>
    public new T Enqueue(T item)
    {
        base.Enqueue(item);
        return Count > FixedCapacity ? Dequeue() : default;
    }
}