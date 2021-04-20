using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pool<T>
{
    private Queue<T> pool = new Queue<T>();
    public HashSet<T> Loaned = new HashSet<T>();

    public Pool() { }

    public void Claim(T poolable)
    {
        if (Loaned.Contains(poolable)) Loaned.Remove(poolable);
        pool.Enqueue(poolable);
    }

    public T Loan()
    {
        if (pool.Count < 1)
        {
            throw new System.Exception("Out of pool space!");
        }
        else
        {
            T poolable = pool.Dequeue();
            Loaned.Add(poolable);
            return poolable;
        }
    }

    public void Clear()
    {
        pool.Clear();
        Loaned.Clear();
    }

    public bool CanLoan() => pool.Count > 0;
}
