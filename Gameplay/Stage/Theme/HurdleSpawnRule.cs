using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[Serializable]
public class HurdleSpawnRule
{
    [SerializeField, MinValue(0f)]
    private float _spawnInterval = 1.2f;
    [SerializeField, MinValue(0f), MaxValue(1f)]
    private float _gap = 0.4f;
    [SerializeField, MinValue(0f)]
    private float _speed = 1f;
    [SerializeField, HideReferenceObjectPicker]
    private StageSpawnEntry<HurdleData>[] _spawnEntries;

    public float SpawnInterval => _spawnInterval;
    public float Gap => _gap;
    public float Speed => _speed;
    public IReadOnlyList<StageSpawnEntry<HurdleData>> SpawnEntries => _spawnEntries;
}
