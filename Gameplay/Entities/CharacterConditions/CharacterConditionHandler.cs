using System;
using System.Collections.Generic;

public class CharacterConditionHandler
{
    private readonly Dictionary<CharacterCondition, int> _countersMap = new();

    public void Add(CharacterCondition condition)
    {
        if (_countersMap.TryGetValue(condition, out int value))
        {
            value++;
        }
        _countersMap[condition] = value;

        if (value == 0)
            Added?.Invoke(condition);
    }

    public void Remove(CharacterCondition condition)
    {
        if (_countersMap.TryGetValue(condition, out int value))
        {
            value--;
            if (value > 0)
            {
                _countersMap[condition] = value;
            }
            else
            {
                _countersMap.Remove(condition);
                Removed?.Invoke(condition);
            }
        }
    }

    public bool Has(CharacterCondition condition)
    {
        return _countersMap.ContainsKey(condition);
    }

    public void Clear()
    {
        foreach (var condition in _countersMap.Keys)
        {
            Removed?.Invoke(condition);
        }
        _countersMap.Clear();
    }

    public event Action<CharacterCondition> Added;
    public event Action<CharacterCondition> Removed;
}