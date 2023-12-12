using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public static class PoolManager
{

    private static Dictionary<int, Stack<IPoolable>> globalPool = new Dictionary<int, Stack<IPoolable>>();

    /*
    public static void Preload<T>(T prefab, int count) where T : MonoBehaviour, IPoolable
    {
        if (prefab == null)
        {
            return;
        }

        int PrefabId = prefab.GetInstanceID();
        int needSpawn = count;
        if (globalPool.TryGetValue(PrefabId, out var pool) && pool != null)
        {
            needSpawn = count - pool.Count;
        }

        for (int i = 0; i < needSpawn; i++)
        {
            T obj = MonoBehaviour.Instantiate(prefab);
            obj.PoolId = prefab.GetInstanceID();
                                
            Push(obj);
        }
    }
   */


    public static T Pool<T>(T prefab) where T : MonoBehaviour, IPoolable
    {
        if (prefab == null)
        {
            return null;
        }

        T obj = null;
        int prefabId = prefab.GetInstanceID();
        if (globalPool.TryGetValue(prefabId, out var pool) && pool != null && pool.Count > 0)
        {
            obj = (T) pool.Pop();
        }

        if (obj == null)
        {
            obj = MonoBehaviour.Instantiate(prefab);
            obj.PoolId = prefab.GetInstanceID();
        }
        obj.gameObject.SetActive(true);
        return obj;
    }

    
    // pool with position
    public static T Pool<T>(T prefab, Vector3 position) where T : MonoBehaviour, IPoolable
    {
        if (prefab == null)
        {
            return null;
        }
        T obj = null;
        int prefabId = prefab.GetInstanceID();
        if (globalPool.TryGetValue(prefabId, out var pool) && pool != null && pool.Count > 0)
        {
            obj = (T) pool.Pop();
        }

        if (obj == null)
        {
            obj = MonoBehaviour.Instantiate(prefab, position, Quaternion.identity);
            obj.PoolId = prefab.GetInstanceID();
        }
        obj.transform.position = position;
        obj.gameObject.SetActive(true);
        return obj;
    }

    
    
    
    
    
    public static void Push<T>(T obj) where T : MonoBehaviour, IPoolable
    {
        if (obj == null)
        {
            return;
        }
        
        obj.gameObject.SetActive(false);
        if (!globalPool.TryGetValue(obj.PoolId, out var pool))
        {
            pool = new Stack<IPoolable>();
            globalPool.Add(obj.PoolId, pool);
        }else if(pool == null)
        {
            pool = new Stack<IPoolable>();
        }

        if (!pool.Contains(obj))
        {
            pool.Push(obj);
        }

    }
    
    ///spec pool
    ///
    public static T Pool<T>(T prefab, Transform parent, bool worldPosStays = true) where T : MonoBehaviour, IPoolable {
        T obj = Pool(prefab);
        if(obj != null) {
            obj.transform.SetParent(parent, worldPosStays);
        }
        return obj;
    }
    
    
    public static T Pool<T>(T prefab, Scene scene) where T : MonoBehaviour, IPoolable {
        T obj = Pool(prefab, null);
        if(obj != null) {
            SceneManager.MoveGameObjectToScene(obj.gameObject, scene);
        }
        return obj;
    }
    
    
}
