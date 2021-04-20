using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoBehaviourPool<T> where T : MonoBehaviour
{
    private GameObject poolObject = new GameObject();
    private Queue<T> pool = new Queue<T>();
    private bool deactivateOnClaim = false;

    private Vector3 poolLocation = new Vector3(0, -10, 0);

    public HashSet<T> Loaned { get; private set; } = new HashSet<T>();

    public MonoBehaviourPool(string name, Transform parent, bool deactivateOnClaim = true)
    {
        poolObject.name = name;
        poolObject.transform.SetParent(parent);
        poolObject.transform.localPosition = poolLocation;
        this.deactivateOnClaim = deactivateOnClaim;
    }

    public void Claim(T item)
    {
        if (Loaned.Contains(item)) Loaned.Remove(item);
        item.transform.SetParent(poolObject.transform);
        item.transform.localPosition = Vector3.zero;
        if (deactivateOnClaim) item.gameObject.SetActive(false);
        pool.Enqueue(item);
    }

    public T Loan()
    {
        if (pool.Count < 1)
        {
            throw new System.Exception("Out of pool space!");
        }
        else
        {
            T item = pool.Dequeue();
            if (deactivateOnClaim) item.gameObject.SetActive(true);
            Loaned.Add(item);
            return item;
        }
    }

    public T Loan(Transform newParent)
    {
        T item = Loan();
        item.transform.SetParent(newParent);
        item.transform.localPosition = Vector3.zero;
        return item;
    }

    public void Clear(bool destroy)
    {
        if (destroy)
        {
            foreach (T item in pool)
            {
                Object.Destroy(item.gameObject);
            }
        }
        pool.Clear();
        Loaned.Clear();
    }

    public bool CanLoan() => pool.Count > 0;
}
