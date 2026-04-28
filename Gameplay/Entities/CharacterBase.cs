using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Sirenix.OdinInspector;

public abstract class CharacterBase : EntityBase
{
    [SerializeField, Required]
    private SpiritController _spiritController;

    private readonly Dictionary<StatType, Stat> _statsMap = new();
    private readonly CharacterConditionHandler _conditionHandler = new();
    private StatusEffectHandler _statusEffectHandler;
    private readonly List<WeaponBase> _weapons = new();

    private CancellationTokenSource _despawnCTS;
    private float _currentHp;
    
    protected void Construct(IEnumerable<KeyValuePair<StatType, Stat>> stats)
    {
        foreach (var kvp in stats)
        {
            _statsMap.Add(kvp.Key, kvp.Value);
        }
        _statusEffectHandler = new(this);
    }

    protected override void OnSpawn()
    {
        _despawnCTS = new();
        DespawnToken = _despawnCTS.Token;
        IsAlive = true;

        CurrentHp = GetStat(StatType.MaxHp).Value;
        foreach (var weapon in _weapons)
        {
            weapon.Equip();
        }
    }

    protected override void OnDespawn()
    {
        _despawnCTS?.Cancel();
        _despawnCTS?.Dispose();
        _despawnCTS = null;
        IsAlive = false;

        foreach (var weapon in _weapons)
        {
            weapon.Unequip();
        }
        foreach (var stat in _statsMap.Values)
        {
            stat.Reset();
        }
        _conditionHandler.Clear();
        _statusEffectHandler.Clear();
    }

    public void Kill()
    {
        if (!IsAlive)
            return;

        IsAlive = false;
        _conditionHandler.Clear();
        OnDie();
        Died?.Invoke();
    }
    
    public void TakeHit(HitResult result)
    {
        if (!IsAlive)
            return;

        if (result.Outcome == HitOutcome.Immune || result.Outcome == HitOutcome.Miss)
        {
            HitTaken?.Invoke(result);
            return;
        }

        OnTakeHit(result);
        CurrentHp -= Mathf.Max(0f, result.Damage);
        HitTaken?.Invoke(result);
        if (IsAlive && CurrentHp <= 0f)
        {
            Kill();
        }
    }
    
    public void AddWeapon(WeaponDataBase weaponData)
    {
        var weapon = Instantiate(weaponData.WeaponPrefab, transform);
        weapon.Construct(this, weaponData);
        _weapons.Add(weapon);

        if (IsAlive)
            weapon.Equip();
    }

    public Stat GetStat(StatType statType)
    {
        return _statsMap[statType];
    }

    
    protected virtual void OnTakeHit(HitResult result) { }
    protected virtual void OnDie() { }

    private void Update()
    {
        if (!IsAlive)
            return;

        _statusEffectHandler.Tick(Time.deltaTime);
    }

    protected CancellationToken DespawnToken { get; private set; }

    public float CurrentHp
    {
        get
        {
            return _currentHp;
        }

        set
        {
            if (!IsAlive)
                return;

            var maxHp = GetStat(StatType.MaxHp).Value;
            _currentHp = Mathf.Max(0f, Mathf.Min(value, maxHp));
            HpChanged?.Invoke(_currentHp);
        }
    }
    
    public bool IsAlive { get; private set; }
    public abstract CharacterType Type { get; }
    public bool IsPlayerCharacter => Type == CharacterType.PlayerCharacter;
    public CharacterConditionHandler Conditions => _conditionHandler;
    public StatusEffectHandler StatusEffects => _statusEffectHandler;
    public SpiritController SpiritController => _spiritController;

    public event Action<float> HpChanged;
    public event Action<HitResult> HitTaken;
    public event Action Died;
}