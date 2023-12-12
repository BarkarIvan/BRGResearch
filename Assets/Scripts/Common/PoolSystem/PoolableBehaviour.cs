using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolableBehaviour : MonoBehaviour, IPoolable
{
    public  int PoolId { get; set; }

    public virtual void PushToPool()
    {
        PoolManager.Push(this);
    }
    
}
