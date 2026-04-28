using System;
using System.Collections.Generic;
using UnityEngine;

public class Stat
{
    private float _initialValue;
    private float _baseValue;
    private readonly List<StatModifier> _modifiers = new();
    private float _cachedValue;
    private bool _isDirty = true;

    public Stat(float baseValue, float minValue, float maxValue)
    {
        float clampedValue = Mathf.Clamp(baseValue, minValue, maxValue);
        _initialValue = _baseValue = _cachedValue = clampedValue;
        MinValue = minValue;
        MaxValue = maxValue;
    }

    public void AddModifier(StatModifier modifier)
    {
        _modifiers.Add(modifier);
        SetDirty();
    }

    public bool RemoveModifier(StatModifier modifier)
    {
        bool removed = _modifiers.Remove(modifier);
        if (removed)
            SetDirty();
        return removed;
    }

    public void RemoveAllModifiersFromSource(object source)
    {
        for (int i = _modifiers.Count - 1; i >= 0; i--)
        {
            if (_modifiers[i].Source == source)
                _modifiers.RemoveAt(i);
        }
        SetDirty();
    }

    public void ClearModifiers()
    {
        _modifiers.Clear();
        SetDirty();
    }

    public void Reset()
    {
        ClearModifiers();
        BaseValue = _initialValue;
    }

    private float CalculateFinalValue()
    {
        float finalValue = _baseValue;
        float percentAddSum = 0f;
        for (int i = 0; i < _modifiers.Count; i++)
        {
            var mod = _modifiers[i];
            switch (mod.Type)
            {
                case StatModifierType.FlatAdd:
                    finalValue += mod.Value;
                    break;
                case StatModifierType.PercentAdd:
                    percentAddSum += mod.Value;
                    break;
                case StatModifierType.PercentMultiply:
                    finalValue *= 1f + mod.Value;
                    break;
            }
        }
        finalValue *= 1f + percentAddSum;
        finalValue = Mathf.Clamp(finalValue, MinValue, MaxValue);
        return finalValue;
    }

    private void SetDirty()
    {
        _isDirty = true;
        if (ValueChanged != null)
        {
            _cachedValue = CalculateFinalValue();
            _isDirty = false;
            ValueChanged.Invoke(_cachedValue);
        }
    }

    public float BaseValue
    {
        get
        {
            return _baseValue;
        }

        set
        {
            _baseValue = value;
            SetDirty();
        }
    }

    public float Value
    {
        get
        {
            if (_isDirty)
            {
                _cachedValue = CalculateFinalValue();
                _isDirty = false;
            }
            return _cachedValue;
        }
    }

    public float MinValue { get; }
    public float MaxValue { get; }

    public event Action<float> ValueChanged;
}