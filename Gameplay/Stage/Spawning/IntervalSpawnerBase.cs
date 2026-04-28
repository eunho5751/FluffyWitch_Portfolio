using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;

public abstract class IntervalSpawnerBase<T> : MonoBehaviour
{
    private CancellationTokenSource _spawningCTS;
    private float _remainingTime;

    public void Dispose()
    {
        StopSpawning();
    }

    public void StartSpawning(T rule, bool repeatSpawning = true)
    {
        if (IsStarted)
        {
            Debug.LogError($"{GetType().Name} has already been started.");
            return;
        }
        IsStarted = true;
        CurrentRule = rule;

        _spawningCTS = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
        SpawnEntities(repeatSpawning, _spawningCTS.Token).Forget();

        OnSpawningStarted();
        SpawningStateChanged?.Invoke(IsStarted);
    }

    public void StopSpawning()
    {
        if (!IsStarted)
            return;
        IsStarted = false;
        CurrentRule = default;
        RemainingTime = 0f;

        _spawningCTS.Cancel();
        _spawningCTS.Dispose();
        _spawningCTS = null;

        OnSpawningStopped();
        SpawningStateChanged?.Invoke(IsStarted);
    }

    protected virtual void OnSpawningStarted() { }
    protected virtual void OnSpawningStopped() { }
    protected abstract void SpawnEntity(T rule);
    protected abstract float GetSpawnInterval(T rule);

    private async UniTaskVoid SpawnEntities(bool repeatSpawning, CancellationToken token)
    {
        RemainingTime = SpawnInterval;
        while (!token.IsCancellationRequested)
        {
            await UniTask.NextFrame(token);
            
            RemainingTime -= Time.deltaTime;
            if (RemainingTime <= 0f)
            {
                SpawnEntity(CurrentRule);

                if (repeatSpawning)
                {
                    RemainingTime = SpawnInterval;
                }
                else
                {
                    StopSpawning();
                }
            }
        }
    }
    
    public bool IsStarted { get; private set; }
    public T CurrentRule { get; private set; }

    public float SpawnInterval => GetSpawnInterval(CurrentRule);
    public float RemainingTime
    {
        get => _remainingTime;
        set => _remainingTime = Mathf.Max(0f, value);
    }

    public event Action<bool> SpawningStateChanged;
}