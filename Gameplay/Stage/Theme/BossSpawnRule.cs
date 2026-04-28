using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[Serializable]
public class BossSpawnRule
{
    [SerializeField, MinValue(0f)]
    private float _spawnInterval = 30f;
    [SerializeField, HideReferenceObjectPicker]
    private StageSpawnEntry<BossData>[] _spawnEntries;

    public float SpawnInterval => _spawnInterval;
    public IReadOnlyList<StageSpawnEntry<BossData>> SpawnEntries => _spawnEntries;
}