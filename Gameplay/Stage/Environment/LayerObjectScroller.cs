using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Pool;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;

using Random = UnityEngine.Random;

public class LayerObjectScroller : MonoBehaviour
{
    [SerializeField, MinValue(0f)]
    private float _initialTimeOffset = 10f;
    [SerializeField, MinValue(0f)]
    private float _moveDistanceOffset;
    [SerializeField, MinValue(0f)]
    private float _spawnRange;
    [SerializeField, MinMaxSlider(0f, 30f, showFields: true)]
    private Vector2 _spawnInterval = new(1f, 2f);
    [SerializeField, MinValue(0f)]
    private float _speed = 1f;
    [SerializeField]
    private GameObject[] _prefabs;
    
#if UNITY_EDITOR
    [Title("Editor Only")]
    [SerializeField]
    private Color _gizmosColor;
#endif

    private ObjectPool<GameObject> _pool;
    private readonly List<GameObject> _activeObjects = new();

    private float _moveDistance;

    private void Awake()
    {
        _pool = new(CreateObject, OnGetObject, OnReleaseObject, OnDestroyObject);
    }

    private async UniTaskVoid Start()
    {
        _moveDistance = Camera.main.orthographicSize * ((float)Camera.main.pixelWidth / Camera.main.pixelHeight) + _moveDistanceOffset;
        
        var token = gameObject.GetCancellationTokenOnDestroy();
        ScrollObjects(token).Forget();

        float remainingInitialTime = _initialTimeOffset;
        while (remainingInitialTime > 0f)
        {
            float spawnInterval = Mathf.Min(remainingInitialTime, Random.Range(_spawnInterval.x, _spawnInterval.y));
            foreach (var activeObj in _activeObjects)
            {
                TryMoveObject(activeObj, spawnInterval);
            }
            remainingInitialTime -= spawnInterval;

            if (remainingInitialTime > 0f)
            {
                var obj = _pool.Get();
                obj.transform.position = GetRandomSpawnPoint();
            }
        }

        while (!token.IsCancellationRequested)
        {
            float spawnInterval = Random.Range(_spawnInterval.x, _spawnInterval.y);
            await UniTask.WaitForSeconds(spawnInterval, cancellationToken: token);

            var obj = _pool.Get();
            obj.transform.position = GetRandomSpawnPoint();
        }
    }

    private void OnDestroy()
    {
        _pool.Dispose();
        _activeObjects.Clear();
    }

    private async UniTaskVoid ScrollObjects(CancellationToken token)
    {
        List<GameObject> objectsToDespawn = new();
        while (!token.IsCancellationRequested)
        {
            foreach (var obj in _activeObjects)
            {
                if (!TryMoveObject(obj, Time.deltaTime))
                {
                    objectsToDespawn.Add(obj);
                }
            }

            foreach (var obj in objectsToDespawn)
            {
                _pool.Release(obj);
            }
            objectsToDespawn.Clear();

            await UniTask.Yield(token);
        }
    }
    
    private bool TryMoveObject(GameObject obj, float deltaTime)
    {
        float despawnPoint = transform.position.x - _moveDistance;
        if (obj.transform.position.x < despawnPoint)
        {
            return false;
        }

        Vector3 targetPos = obj.transform.position + Vector3.left * (deltaTime * _speed);
        obj.transform.position = targetPos;
        return true;
    }

    private Vector2 GetRandomSpawnPoint()
    {
        Vector2 spawnPoint = transform.position + Vector3.right * _moveDistance;
        spawnPoint.y += Random.Range(-_spawnRange, _spawnRange) * 0.5f;
        return spawnPoint;
    }

    private GameObject CreateObject()
    {
        var obj = Instantiate(_prefabs[Random.Range(0, _prefabs.Length)], transform);
        obj.SetActive(false);
        return obj;
    }

    private void OnGetObject(GameObject obj)
    {
        obj.SetActive(true);
        _activeObjects.Add(obj);

        ObjectSpawned?.Invoke(obj);
    }

    private void OnReleaseObject(GameObject obj)
    {
        obj.SetActive(false);
        _activeObjects.Remove(obj);

        ObjectDespawned?.Invoke(obj);
    }

    private void OnDestroyObject(GameObject obj)
    {
        Destroy(obj);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Color c = Gizmos.color;
        Gizmos.color = _gizmosColor;

        float moveDistance = Camera.main.orthographicSize * ((float)Camera.main.pixelWidth / Camera.main.pixelHeight) + _moveDistanceOffset;
        Vector3 despawnPoint = transform.position + Vector3.left * moveDistance;
        Vector3 spawnPoint = transform.position + Vector3.right * moveDistance;
        Gizmos.DrawLine(despawnPoint, spawnPoint);

        Vector3 spawnRangeMin = spawnPoint + Vector3.down * (_spawnRange * 0.5f);
        Vector3 spawnRangeMax = spawnPoint + Vector3.up * (_spawnRange * 0.5f);
        Gizmos.DrawLine(spawnRangeMin, spawnRangeMax);

        Gizmos.color = c;
    }
#endif

    public event Action<GameObject> ObjectSpawned;
    public event Action<GameObject> ObjectDespawned;
}