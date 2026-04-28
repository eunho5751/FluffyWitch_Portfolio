using System;
using UnityEngine;
using Sirenix.OdinInspector;

[Serializable]
public class StageSpawnEntry<T> where T : GuidScriptableObjectBase
{
    [InlineProperty, HideReferenceObjectPicker]
    [SerializeField, Required]
    private GuidRef<T> _entity;
    [SerializeField, MinValue(0f)]
    private float _weight;

    public string Guid => _entity.Guid;
    public float Weight => _weight;
}