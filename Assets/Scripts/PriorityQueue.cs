using System;
using System.Collections.Generic;

// Many ways of doing this (exists in C# 9.0 but not the Unity packaged one)
// Followed the tutorial here --> https://www.youtube.com/watch?v=xDadpPBq0Bw
// Allows to use a queue with "weights", i.e. a chained list with a default order of First-In-First-Out if equal weights
// 0/1 is VERY HIGH priority

public class PriorityQueue<T>
{
    private List<Tuple<T, int>> elements = new List<Tuple<T, int>>();

    public int Count
    {
        get { return elements.Count; }
    }

    public void Enqueue (T item, int priority)
    {
        elements.Add(Tuple.Create(item, priority));
    }

    public T Dequeue ()
    {
        int bestIndex = 0;

        for (int i = 0; i < elements.Count; i++) {
            if (elements[i].Item2 < elements[bestIndex].Item2) {
                bestIndex = i;
            }
        }

        T bestItem = elements[bestIndex].Item1;
        elements.RemoveAt(bestIndex);
        return bestItem;
    }
}
