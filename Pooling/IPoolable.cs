using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPoolable
{
    /*HashSet<object> Loaned { get; set; }
    Queue Pool { get; set; }

    public void Reclaim(object poolable);

    public object Loan();

    public void Clear();

    public bool CanLoan();*/
}

public enum ObjectType
{
    Entity,
    Model,
    Path
}
