using System;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class PlayerCharacter : CharacterBase
{
    private float _initialColliderScale;
    private int _currentLevel;
    private int _currentExp;

    private float _lastJumpTime;
    private CancellationTokenSource _hoverCTS;

    private CancellationTokenSource _hpRegenCTS;

    private void OnDestroy()
    {
        CancelAndDispose(ref _hoverCTS);
        CancelAndDispose(ref _hpRegenCTS);
    }

    public void Construct(PlayerCharacterData characterData)
    {
        base.Construct(characterData.CreateStats());
        Data = characterData;
        CurrentLevel = 1;
        
        var hitboxScale = GetStat(StatType.HitboxScale);
        hitboxScale.ValueChanged += OnHitboxScaleChanged;
        _initialColliderScale = ((CircleCollider2D)Collider).radius;

        foreach (var weaponGuid in Data.InitialWeapons)
        {
            var initialWeapon = GameManager.Instance.Database.GetWeaponData(weaponGuid);
            AddWeapon(initialWeapon);
        }
    }

    public void AddExp(int value)
    {
        int newExp = CurrentExp + Mathf.FloorToInt(value * (1f + GetStat(StatType.ExpGainMultiplier).Value));
        while (newExp >= NextLevelUpExp)
        {
            newExp -= NextLevelUpExp;
            LevelUp();
        }
        CurrentExp = newExp;
    }

    public void LevelUp()
    {
        CurrentLevel++;
    }

    public void Jump()
    {
        if (Conditions.Has(CharacterCondition.JumpLock))
            return;

        CancelAndDispose(ref _hoverCTS);
        float timeDiff = Time.time - _lastJumpTime;
        bool canHover = !IsHovering && timeDiff < Data.Common.HoverThreshold;
        if (canHover)
        {
            _hoverCTS = new();
            ApplyHovering(_hoverCTS.Token).Forget();
        }
        else
        {
            SetVelocity(Data.Common.JumpForce);
            _lastJumpTime = Time.time;
        }
    }
    protected override void OnSpawn()
    {
        base.OnSpawn();
        
        _lastJumpTime = -Data.Common.HoverThreshold;

        _hpRegenCTS = CancellationTokenSource.CreateLinkedTokenSource(DespawnToken);
        HpRegenLoop(_hpRegenCTS.Token).Forget();
    }

    protected override void OnDespawn()
    {
        base.OnDespawn();

        gameObject.SetActive(false);
    }

    protected override void OnMove(ref Vector2 velocity)
    {
        if (!IsHovering)
        {
            ApplyGravity(ref velocity);
        }
    }

    protected override void OnTakeHit(HitResult result)
    {
        if (result.DamageKind == DamageKind.Impact)
            StatusEffects.Apply(StatusEffectRegistry.HitInvincibility);
    }

    protected override void OnDie()
    {
        _hpRegenCTS.Cancel();
        _hpRegenCTS.Dispose();
        _hpRegenCTS = null;

        Despawn();
    }

    private void OnHitboxScaleChanged(float value)
    {
        ((CircleCollider2D)Collider).radius = _initialColliderScale * (1f - value * 0.5f);
    }

    private async UniTaskVoid HpRegenLoop(CancellationToken token)
    {
        var hpRegen = GetStat(StatType.HpRegen);
        while (!token.IsCancellationRequested)
        {
            await UniTask.WaitForSeconds(Data.Common.HpRegenInterval, cancellationToken: token);
            CurrentHp += hpRegen.Value;
        }
    }
    
    private void ApplyGravity(ref Vector2 velocity)
    {
        float gravity = Data.Common.Gravity;
        if (velocity.y < 0f)
            gravity *= Data.Common.FallMultiplier;
        velocity.y += gravity * (1f + GetStat(StatType.GravityScale).Value) * Time.fixedDeltaTime;
    }

    private void SetVelocity(float vel)
    {
        var newVel = Velocity;
        newVel.y = vel;
        Velocity = newVel;
    }

    private async UniTaskVoid ApplyHovering(CancellationToken token)
    {
        try
        {
            IsHovering = true;
            SetVelocity(0f);
            await UniTask.WaitForSeconds(Data.Common.HoverDuration, cancellationToken: token, cancelImmediately: true);
        }
        finally
        {
            IsHovering = false;
        }
    }

    private void CancelAndDispose(ref CancellationTokenSource cts)
    {
        if (cts == null)
            return;
        cts.Cancel();
        cts.Dispose();
        cts = null;
    }

    public int CurrentLevel
    {
        get
        {
            return _currentLevel;
        }

        private set
        {
            _currentLevel = value;
            NextLevelUpExp = Mathf.RoundToInt(Data.Common.InitialLevelUpExp * Mathf.Pow(Data.Common.LevelUpExpMultiplier, _currentLevel));
            LevelChanged?.Invoke(_currentLevel);
        }
    }

    public int CurrentExp
    {
        get
        {
            return _currentExp;
        }

        private set
        {
            _currentExp = value;
            ExpChanged?.Invoke(_currentExp);
        }
    }

    public bool IsMaxLevel => CurrentLevel >= Data.Common.MaxLevel;
    public int NextLevelUpExp { get; private set; }

    public bool IsJumping => Velocity.y > 0f;
    public bool IsHovering { get; private set; }

    public PlayerCharacterData Data { get; private set; }
    public override CharacterType Type => CharacterType.PlayerCharacter;

    public event Action<int> LevelChanged;
    public event Action<int> ExpChanged;
}