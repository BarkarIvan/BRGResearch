
using System;
using UnityEngine;
using UnityEngine.Rendering.UI;
using Random = UnityEngine.Random;

public class AutoPushPoolableObject : MonoBehaviour
{
    public PoolableBehaviour poolableObject;
    public float minLifeTime = 2f;
    public float maxLifeTime = 2f;
    private float _randomLifeTime;

    private float _enableTime;


    private void OnEnable()
    {
        SetRandomLifeTime();
        _enableTime = Time.time;
    }


    private void SetRandomLifeTime()
    {
        _randomLifeTime = Random.Range(minLifeTime, maxLifeTime);//4
        
    }

    private bool IsOnLifeTime()
    {
        if (Time.time - _enableTime > _randomLifeTime) //10-8
        {
            return false;
        }
            return true;
    }

    private void Update()
    {
        if (!IsOnLifeTime())
        {
            PoolManager.Push(poolableObject);
        }
    }
}
