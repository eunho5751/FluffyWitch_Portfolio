using UnityEngine;

public class Enemy : CharacterBase
{
    private EntityPool _pool;
    private MovePatternBase _movePattern;
    private Stat _collisionDamage;
    private Stat _moveSpeed;

    private bool _isConsturcted;
    private int _currentStage;

    public void Construct(EnemyData enemyData, EntityPool pool)
    {
        if (_isConsturcted)
            return;

        base.Construct(enemyData.CreateStats());

        Data = enemyData;
        _pool = pool;
        _movePattern = enemyData.MovePattern.CreateOrGetInstance(this);

        _collisionDamage = GetStat(StatType.CollisionDamage);
        _moveSpeed = GetStat(StatType.MoveSpeed);

        foreach (var weaponGuid in Data.InitialWeapons)
        {
            var initialWeapon = GameManager.Instance.Database.GetWeaponData(weaponGuid);
            AddWeapon(initialWeapon);
        }

        _isConsturcted = true;
    }

    public void Set(int currentStage)
    {
        _currentStage = currentStage;
    }

    protected override void OnSpawn()
    {
        base.OnSpawn();
        
        _movePattern.ResetState(this);
        ApplyStageGrowth(_currentStage);
    }

    protected override void OnDespawn()
    {
        base.OnDespawn();
        
        _pool.ReleaseEnemy(this);
    }

    protected override void OnMove(ref Vector2 velocity)
    {
        MoveContext moveCtx = new()
        {
            Character = this,
            Speed = _moveSpeed.Value
        };
        velocity = _movePattern.Evaluate(moveCtx);
    }

    protected override void OnDie()
    {
        Despawn();
    }

    protected override void OnCollision(EntityBase entity)
    {
        if (entity.TryGetComponent(out PlayerCharacter playerCharacter))
        {
            HitContext damageCtx = HitContext.Impact(_collisionDamage.Value, DamageFlags.None, this, playerCharacter);
            CombatSystem.Instance.TryApplyHit(damageCtx, out _);
        }
    }

    private void ApplyStageGrowth(int stage)
    {
        foreach (var growth in Data.StatGrowthsPerStage)
        {
            StatModifier modifier = new(growth.Value * (stage - 1), StatModifierType.FlatAdd);
            var stat = GetStat(growth.Key);
            stat.AddModifier(modifier);
        }

        CurrentHp = GetStat(StatType.MaxHp).Value;
    }

    public EnemyData Data { get; private set; }
    public override CharacterType Type => CharacterType.Enemy;
}