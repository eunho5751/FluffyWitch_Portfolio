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
    private readonly bool _isInteger;

    public Stat(float baseValue, float minValue, float maxValue, bool isInteger = false)
    {
        float clampedValue = Mathf.Clamp(baseValue, minValue, maxValue);
        _initialValue = _baseValue = _cachedValue = clampedValue;
        MinValue = minValue;
        MaxValue = maxValue;
        _isInteger = isInteger;
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
        float flatSum = 0f;
        float percentAddSum = 0f;
        float percentMul = 1f;

        for (int i = 0; i < _modifiers.Count; i++)
        {
            var mod = _modifiers[i];
            switch (mod.Type)
            {
                case StatModifierType.FlatAdd:         flatSum       += mod.Value;          break;
                case StatModifierType.PercentAdd:      percentAddSum += mod.Value;          break;
                case StatModifierType.PercentMultiply: percentMul    *= 1f + mod.Value;     break;
            }
        }

        float finalValue = (_baseValue + flatSum) * (1f + percentAddSum) * percentMul;
        finalValue = Mathf.Clamp(finalValue, MinValue, MaxValue);
        // 정수 차원 스탯은 모든 모디파이어 합산 후 최종값에서 한 번만 내림한다 (모디파이어별 내림 금지)
        return _isInteger ? Mathf.Floor(finalValue) : finalValue;
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