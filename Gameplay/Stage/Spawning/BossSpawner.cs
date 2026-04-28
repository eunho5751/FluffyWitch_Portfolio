using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Random = UnityEngine.Random;

public class BossSpawner : IntervalSpawnerBase<BossSpawnRule>
{
    [SerializeField, MaxValue(0f)]
    private float _spawnPoint = -0.1f;

    private Vector2 _spawnPosition;

    public void Construct()
    {
        _spawnPosition = GetSpawnPosition();
    }

    protected override void OnSpawningStarted()
    {
        CombatSystem.Instance.Killed += OnCharacterKilled;
    }

    protected override void OnSpawningStopped()
    {
        CombatSystem.Instance.Killed -= OnCharacterKilled;
    }

    protected override void SpawnEntity(BossSpawnRule rule)
    {
        if (rule.SpawnEntries.Count == 0)
            return;
            
        int currentStage = StageManager.Instance.CurrentStage;
        var entities = StageManager.Instance.Entities;
        var db = GameManager.Instance.Database;

        int entryIndex = PickWeightedRandomEntity(rule.SpawnEntries);
        var bossGuid = rule.SpawnEntries[entryIndex].Guid;
        var bossData = db.GetEnemyData(bossGuid);
        var boss = entities.GetEnemy(bossData);
        boss.Set(currentStage);
        boss.Spawn(_spawnPosition);

        Spawned?.Invoke(boss);
    }

    protected override float GetSpawnInterval(BossSpawnRule rule)
    {
        return rule.SpawnInterval;
    }

    private int PickWeightedRandomEntity(IReadOnlyList<StageSpawnEntry<BossData>> bosses)
    {
        float total = 0f;
        foreach (var e in bosses)
            total += e.Weight;

        float random = Random.value;
        float cumulative = 0f;
        for (int i = 0; i < bosses.Count; i++)
        {
            cumulative += bosses[i].Weight / total;
            if (random <= cumulative)
                return i;
        }

        return bosses.Count - 1;
    }

    private Vector2 GetSpawnPosition()
    {
        float spawnWorldPos = Camera.main.ViewportToWorldPoint(new Vector2(1f + _spawnPoint, 0f)).x;
        return new(spawnWorldPos, 0f);
    }

    private void OnCharacterKilled(CharacterBase attacker, CharacterBase victim)
    {
        if (victim.Type == CharacterType.Enemy)
        {
            RemainingTime -= StageManager.Instance.StageConfig.BossSpawnTimeReductionPerKill;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        var camera = Camera.main;
        if (camera == null)
            return;

        Color c = Gizmos.color;
        Gizmos.color = Color.hotPink;

        Vector2 spawnPosition = camera.ViewportToWorldPoint(new Vector2(1f + _spawnPoint, 0.5f));
        Gizmos.DrawWireSphere(spawnPosition, 0.25f);

        Gizmos.color = c;
    }
#endif

    public event Action<Enemy> Spawned;
}