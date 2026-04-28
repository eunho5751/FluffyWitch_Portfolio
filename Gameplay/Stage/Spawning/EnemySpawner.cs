using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : IntervalSpawnerBase<EnemySpawnRule>
{
    [SerializeField]
    private float _spawnPoint = 0.1f;
    [SerializeField, Range(0f, 1f)]
    private float _spawnRange = 0.9f;

    public void Construct()
    {
    }

    protected override void SpawnEntity(EnemySpawnRule rule)
    {
        if (rule.SpawnEntries.Count == 0)
            return;
            
        int currentStage = StageManager.Instance.CurrentStage;
        var entities = StageManager.Instance.Entities;
        var db = GameManager.Instance.Database;
        for (int i = 0; i < rule.SpawnCount; i++)
        {
            var spawnPosition = GetSpawnPosition();
            int entryIndex = PickWeightedRandomEntity(rule.SpawnEntries);
            var enemyGuid = rule.SpawnEntries[entryIndex].Guid;
            var enemyData = db.GetEnemyData(enemyGuid);
            var enemy = entities.GetEnemy(enemyData);
            enemy.Set(currentStage);
            enemy.Spawn(spawnPosition);
        }
    }

    protected override float GetSpawnInterval(EnemySpawnRule rule)
    {
        return rule.SpawnInterval;
    }

    private int PickWeightedRandomEntity(IReadOnlyList<StageSpawnEntry<EnemyData>> enemies)
    {
        float total = 0f;
        foreach (var e in enemies)
            total += e.Weight;

        float random = Random.value;
        float cumulative = 0f;
        for (int i = 0; i < enemies.Count; i++)
        {
            cumulative += enemies[i].Weight / total;
            if (random <= cumulative)
                return i;
        }

        return enemies.Count - 1;
    }

    private Vector2 GetSpawnPosition()
    {
        float x = 1f + _spawnPoint;
        float y = Random.Range(0f, _spawnRange) + (1f - _spawnRange) * 0.5f;
        Vector2 spawnPosition = Camera.main.ViewportToWorldPoint(new Vector2(x, y));
        return spawnPosition;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        var camera = Camera.main;
        if (camera == null)
            return;

        Color c = Gizmos.color;
        Gizmos.color = Color.seaGreen;

        Vector2 spawnPosition = camera.ViewportToWorldPoint(new Vector2(1f + _spawnPoint, 0.5f));
        Gizmos.DrawWireSphere(spawnPosition, 0.25f);

        float yMin = (1f - _spawnRange) * 0.5f;
        float yMax = yMin + _spawnRange;
        Vector2 spawnRangeMin = camera.ViewportToWorldPoint(new(0f, yMin));
        Vector2 spawnRangeMax = camera.ViewportToWorldPoint(new(0f, yMax));
        spawnRangeMin.x = spawnRangeMax.x = spawnPosition.x;
        Gizmos.DrawLine(spawnRangeMin, spawnRangeMax);

        Gizmos.color = c;
    }
#endif
}