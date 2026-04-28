using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class HurdleSpawner : IntervalSpawnerBase<HurdleSpawnRule>
{
    [SerializeField, MinValue(0f)]
    private float _spawnPoint = 0.1f;

    private float _maxHeight;
    private Vector2 _spawnPosition;

    public void Construct()
    {
        _spawnPosition = GetSpawnPosition();
        _maxHeight = Camera.main.orthographicSize * 2f;
    }

    protected override void SpawnEntity(HurdleSpawnRule rule)
    {
        if (rule.SpawnEntries.Count == 0)
            return;

        var gameMgr = GameManager.Instance;
        var stageMgr = StageManager.Instance;

        int entryIndex = PickWeightedRandomEntity(rule.SpawnEntries);
        var hurdleGuid = rule.SpawnEntries[entryIndex].Guid;
        var hurdleData = gameMgr.Database.GetHurdleData(hurdleGuid);
        var hurdle = stageMgr.Entities.GetHurdle(hurdleData);
        int currentStage = stageMgr.CurrentStage;
        float basePassLine = stageMgr.PlayerCharacter.Position.x;
        hurdle.Set(currentStage, basePassLine, _maxHeight, rule.Gap, rule.Speed);
        hurdle.Spawn(_spawnPosition);
    }

    protected override float GetSpawnInterval(HurdleSpawnRule rule)
    {
        return rule.SpawnInterval;
    }

    private int PickWeightedRandomEntity(IReadOnlyList<StageSpawnEntry<HurdleData>> hurdles)
    {
        float total = 0f;
        foreach (var e in hurdles)
            total += e.Weight;

        float random = Random.value;
        float cumulative = 0f;
        for (int i = 0; i < hurdles.Count; i++)
        {
            cumulative += hurdles[i].Weight / total;
            if (random <= cumulative)
                return i;
        }

        return hurdles.Count - 1;
    }

    private Vector2 GetSpawnPosition()
    {
        float spawnWorldPos = Camera.main.ViewportToWorldPoint(new Vector2(1f + _spawnPoint, 0f)).x;
        return new(spawnWorldPos, 0f);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        var camera = Camera.main;
        if (camera == null)
            return;

        Color c = Gizmos.color;
        Gizmos.color = Color.thistle;

        Vector2 spawnPosition = camera.ViewportToWorldPoint(new Vector2(1f + _spawnPoint, 0.5f));
        Gizmos.DrawWireSphere(spawnPosition, 0.25f);

        Gizmos.color = c;
    }
#endif
}