using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    private const float CreditTickInterval = 0.25f;

    [SerializeField]
    private float _spawnPoint = 0.1f;
    [SerializeField, Range(0f, 1f)]
    private float _spawnRange = 0.9f;

    private EnemySpawnTable _spawnTable;
    private float _creditPerSecond;
    private float _creditTickTimer;
    private EnemyData _reservedEnemy;
    private int _reservedCost;
    private float _drainMargin;
    private int _maxSpawnPerFrame;
    private float _drainPerSecond;
    private float _drainAccumulator;
    private readonly Queue<EnemyData> _purchaseQueue = new();

    public void Construct(EnemySpawnTable spawnTable, float drainMargin, int maxSpawnPerFrame)
    {
        _spawnTable = spawnTable;
        _drainMargin = drainMargin;
        _maxSpawnPerFrame = maxSpawnPerFrame;
    }

    public void StartSpawning()
    {
        IsStarted = true;
    }

    public void StopSpawning()
    {
        IsStarted = false;
    }

    public void ApplyCreditRate(float creditPerSecond)
    {
        _creditPerSecond = creditPerSecond;
        // 방출 속도는 구매 속도(수급률/평균 코스트)에 여유 배수를 곱해 파생한다 —
        // 유입보다 항상 빠르므로 평상시 큐는 몇 틱 안에 비워진다.
        _drainPerSecond = creditPerSecond / _spawnTable.AvgCost * _drainMargin;
    }

    public void ResetState()
    {
        Credits = 0f;
        _creditTickTimer = 0f;
        _reservedEnemy = null;
        _drainAccumulator = 0f;
        _purchaseQueue.Clear();
    }

    private void Update()
    {
        if (!IsStarted)
            return;
        // 일시정지(timeScale = 0) 중에는 수급·스폰 모두 정지한다
        float deltaTime = Time.deltaTime;
        if (deltaTime <= 0f)
            return;

        // 난이도 스폰 배율은 사용 시점에 읽어 스테이지 중간 변화(패시브 획득·강화)도 즉시 반영한다.
        // 수급·방출 양쪽에 곱해야 "방출 = 구매 × 여유 배수" 불변식이 유지된다.
        float rateMultiplier = RateMultiplier;

        // 고정 틱 수급 — 완성된 틱만큼 반복 획득하고 잔여 시간은 다음 계산으로 이월한다
        _creditTickTimer += deltaTime;
        while (_creditTickTimer >= CreditTickInterval)
        {
            _creditTickTimer -= CreditTickInterval;
            Credits += _creditPerSecond * rateMultiplier * CreditTickInterval;
            PurchaseEnemies();
        }

        SpawnQueuedEnemies(deltaTime, rateMultiplier);
    }

    // 가중치 추첨으로 뽑힌 몬스터를 예약해두고 크레딧이 모이면 구매한다 —
    // 못 사도 예약이 다음 틱까지 유지되므로 비싼 몬스터도 추첨 확률만큼 확실하게 등장한다.
    private void PurchaseEnemies()
    {
        if (!_spawnTable.HasSpawnableEnemies)
            return;

        while (true)
        {
            if (_reservedEnemy == null)
                _reservedEnemy = _spawnTable.PickRandomEnemy(out _reservedCost);

            if (Credits < _reservedCost)
                return;

            Credits -= _reservedCost;
            _purchaseQueue.Enqueue(_reservedEnemy);
            _reservedEnemy = null;
        }
    }

    private void SpawnQueuedEnemies(float deltaTime, float rateMultiplier)
    {
        // 방출 예산은 시간 기준으로 적립한다 — 프레임레이트와 무관하게 초당 방출 수가 일정하고,
        // 프레임당 상한은 인스턴스화 성능 예산으로만 걸린다 (버스트를 여러 프레임으로 스무딩).
        _drainAccumulator += _drainPerSecond * rateMultiplier * deltaTime;
        int budget = Mathf.Min(Mathf.Min((int)_drainAccumulator, _maxSpawnPerFrame), _purchaseQueue.Count);
        if (budget <= 0)
            return;
        _drainAccumulator -= budget;

        var stageManager = StageManager.Instance;
        for (int i = 0; i < budget; i++)
        {
            var enemyData = _purchaseQueue.Dequeue();
            stageManager.SpawnEnemy(enemyData, GetSpawnPosition());
        }
    }

    private Vector2 GetSpawnPosition()
    {
        float x = 1f + _spawnPoint;
        float y = Random.Range(0f, _spawnRange) + (1f - _spawnRange) * 0.5f;
        Vector2 spawnPosition = Camera.main.ViewportToWorldPoint(new Vector2(x, y));
        return spawnPosition;
    }

    private float RateMultiplier => StageManager.Instance.Difficulty.EnemySpawnRateMultiplier.Value;

    public bool IsStarted { get; private set; }
    public float Credits { get; private set; }
    public int QueueCount => _purchaseQueue.Count;
}
