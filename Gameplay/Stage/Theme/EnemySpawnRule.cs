using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[Serializable]
public class EnemySpawnRule
{
    [SerializeField, MinValue(0f)]
    private float _spawnInterval = 0.8333f;
    [SerializeField, MinValue(1)]
    private int _spawnCount = 1;
    [SerializeField, HideReferenceObjectPicker]
    private StageSpawnEntry<EnemyData>[] _spawnEntries;

    public float SpawnInterval => _spawnInterval;
    public int SpawnCount => _spawnCount;
    public IReadOnlyList<StageSpawnEntry<EnemyData>> SpawnEntries => _spawnEntries;
}
