using System.Collections.Generic;
using UnityEngine;

// 테마 로스터의 몬스터를 EnemyData까지 캐싱해두고(테마 변경 시 Rebuild), 스테이지 변경 시
// 가중치 커브를 Evaluate해 추첨 가중치와 평균 코스트를 갱신한다. 스폰 핫패스는 배열 스캔만 수행한다.
public class EnemySpawnTable
{
    private EnemyData[] _enemies;
    private AnimationCurve[] _weightCurves;
    private int[] _costs;
    private float[] _weights;
    private float _totalWeight;
    private float _avgCost;

    public void Rebuild(StageThemeData theme, Database db)
    {
        var roster = theme.EnemyRoster;
        List<EnemyData> enemies = new();
        List<AnimationCurve> weightCurves = new();
        List<int> costs = new();
        if (roster != null)
        {
            for (int i = 0; i < roster.Count; i++)
            {
                if (!db.TryGetEnemyData(roster[i].Guid, out var enemyData))
                {
                    Debug.LogError($"{theme.name}: enemy roster entry has invalid guid!");
                    continue;
                }

                // 코스트 0은 무한 구매를 만들므로 최소 1로 보정한다
                int cost = roster[i].Cost;
                if (cost < 1)
                {
                    Debug.LogError($"{theme.name}: {enemyData.name} cost must be at least 1!");
                    cost = 1;
                }

                enemies.Add(enemyData);
                weightCurves.Add(roster[i].WeightCurve);
                costs.Add(cost);
            }
        }

        _enemies = enemies.ToArray();
        _weightCurves = weightCurves.ToArray();
        _costs = costs.ToArray();
        _weights = new float[_enemies.Length];
        _totalWeight = 0f;
    }

    public void ApplyStage(int localStage)
    {
        _totalWeight = 0f;
        float weightedCostSum = 0f;
        for (int i = 0; i < _weights.Length; i++)
        {
            float weight = Mathf.Max(0f, _weightCurves[i].Evaluate(localStage));
            _weights[i] = weight;
            _totalWeight += weight;
            weightedCostSum += _costs[i] * weight;
        }

        // 뽑힐 확률을 반영한 평균 구매 코스트 — 방출 속도 산출에 쓰인다
        if (_totalWeight > 0f)
        {
            _avgCost = weightedCostSum / _totalWeight;
        }
        else
        {
            Debug.LogWarning($"EnemySpawnTable: no spawnable enemies at local stage {localStage}! (total weight is 0)");
            _avgCost = 1f;
        }
    }

    public EnemyData PickRandomEnemy(out int cost)
    {
        float random = Random.value * _totalWeight;
        float cumulative = 0f;
        int picked = -1;
        for (int i = 0; i < _weights.Length; i++)
        {
            if (_weights[i] <= 0f)
                continue;

            // 부동소수 오차로 random이 누적 합을 넘어도 마지막 유효 몬스터로 수렴한다
            picked = i;
            cumulative += _weights[i];
            if (random <= cumulative)
                break;
        }

        cost = _costs[picked];
        return _enemies[picked];
    }

    public bool HasSpawnableEnemies => _totalWeight > 0f;
    public float AvgCost => _avgCost;
}
