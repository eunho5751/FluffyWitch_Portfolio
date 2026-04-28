using System;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectHandler
{
    private readonly List<StatusEffectBase> _activeEffects = new();

    public StatusEffectHandler(CharacterBase owner)
    {
        Owner = owner;
    }

    public void Apply(StatusEffectDataBase data)
    {
        Apply(null, data);
    }

    public void Apply(CharacterBase caster, StatusEffectDataBase data)
    {
        if (!Owner.IsAlive)
            return;

        var existing = FindInstance(data);
        if (existing != null)
        {
            existing.AddStack();
            return;
        }

        var instance = data.CreateInstance();
        instance.Initialize(caster, Owner);
        _activeEffects.Add(instance);
        instance.OnApply();
        Applied?.Invoke(instance);
    }

    public void Remove(StatusEffectDataBase data)
    {
        var instance = FindInstance(data);
        if (instance == null)
            return;

        _activeEffects.Remove(instance);
        instance.OnRemove();
        Removed?.Invoke(instance);
    }

    public void Clear()
    {
        for (int i = _activeEffects.Count - 1; i >= 0; i--)
        {
            _activeEffects[i].OnRemove();
            Removed?.Invoke(_activeEffects[i]);
        }
        _activeEffects.Clear();
    }

    public void Tick(float deltaTime)
    {
        int cnt = _activeEffects.Count;
        while (cnt > 0)
        {
            int idx = cnt - 1;
            var instance = _activeEffects[idx];
            instance.Tick(deltaTime);
            if (_activeEffects.Contains(instance) && instance.IsExpired)
            {
                _activeEffects.RemoveAt(idx);
                instance.OnRemove();
                Removed?.Invoke(instance);
            }
            cnt = Mathf.Min(cnt - 1, _activeEffects.Count);
        }
    }

    public bool Has(StatusEffectDataBase data)
    {
        return FindInstance(data) != null;
    }

    public bool Has<T>() where T : StatusEffectBase
    {
        for (int i = 0; i < _activeEffects.Count; i++)
        {
            if (_activeEffects[i] is T)
                return true;
        }
        return false;
    }

    private StatusEffectBase FindInstance(StatusEffectDataBase data)
    {
        for (int i = 0; i < _activeEffects.Count; i++)
        {
            if (_activeEffects[i].Data == data)
                return _activeEffects[i];
        }
        return null;
    }

    public CharacterBase Owner { get; private set; }
    public IReadOnlyList<StatusEffectBase> ActiveEffects => _activeEffects;

    public event Action<StatusEffectBase> Applied;
    public event Action<StatusEffectBase> Removed;
}
