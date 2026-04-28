using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : SingletonBehaviourBase<PoolManager>
{
    private readonly Dictionary<GameObject, ObjectPool<GameObject>> _prefabToPoolMap = new();
    private readonly Dictionary<GameObject, ObjectPool<GameObject>> _instanceToPoolMap = new();

    private void OnDestroy()
    {
        foreach (var instance in _instanceToPoolMap.Keys)
        {
            Destroy(instance);
        }
    }

    public T Get<T>(GameObject prefabKey) where T : MonoBehaviour
    {
        var instance = Get(prefabKey);
        instance.TryGetComponent(out T component);
        return component;
    }

    public GameObject Get(GameObject prefabKey)
    {
        if (prefabKey == null)
            throw new ArgumentNullException(nameof(prefabKey));

        var pool = GetPool(prefabKey);
        var instance = pool.Get();
        _instanceToPoolMap.Add(instance, pool);
        return instance;
    }

    public void Release(GameObject instance)
    {
        if (instance == null)
        {
            Debug.LogError("Can't release null object.");
            return;
        }

        if (!_instanceToPoolMap.TryGetValue(instance, out var pool))
        {
            Debug.LogError("Object was not spawned from this pool.");
            return;
        }

        pool.Release(instance);
        _instanceToPoolMap.Remove(instance);
    }

    public void Clear()
    {
        foreach (var instance in _instanceToPoolMap.Keys)
        {
            if (instance != null)
            {
                Destroy(instance);
            }
        }
        _instanceToPoolMap.Clear();

        foreach (var pool in _prefabToPoolMap.Values)
        {
            pool.Clear();
        }
        _prefabToPoolMap.Clear();

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var poolParent = transform.GetChild(i).gameObject;
            Destroy(poolParent);
        }
    }

    private ObjectPool<GameObject> GetPool(GameObject prefabKey)
    {
        if (prefabKey == null)
            throw new ArgumentNullException(nameof(prefabKey));

        if (!_prefabToPoolMap.TryGetValue(prefabKey, out var pool))
        {
            pool = CreatePool(prefabKey);
            _prefabToPoolMap.Add(prefabKey, pool);
        }
        return pool;
    }

    private ObjectPool<GameObject> CreatePool(GameObject prefab)
    {
        GameObject parent = new($"{prefab.name} Pool");
        parent.transform.SetParent(transform);

        ObjectPool<GameObject> newPool = new(
            createFunc: () => CreateObject(prefab, parent.transform),
            actionOnRelease: instance => OnReleaseObject(instance, parent.transform), 
            actionOnDestroy: OnDestroyObject);
        return newPool;
    }

    private GameObject CreateObject(GameObject prefab, Transform parent)
    {
        var instance = Instantiate(prefab, parent);
        instance.SetActive(false);
        return instance;
    }

    private void OnReleaseObject(GameObject instance, Transform parent)
    {
        instance.transform.SetParent(parent, false);
        instance.SetActive(false);
    }

    private void OnDestroyObject(GameObject instance)
    {
        if (instance != null)
            Destroy(instance);
    }
}