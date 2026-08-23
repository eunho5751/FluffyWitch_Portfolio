using System;
using UnityEngine;
using Sirenix.OdinInspector;

[Serializable, HideReferenceObjectPicker]
public class StatDefinition
{
    [SerializeField, MaxValue("$_maxValue")]
    private float _minValue;
    [SerializeField, MinValue("$_minValue")]
    private float _maxValue;

    public float MinValue => _minValue;
    public float MaxValue => _maxValue;
}
