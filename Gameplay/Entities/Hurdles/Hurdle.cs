using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Hurdle : EntityBase
{
    [SerializeField]
    private Transform _top;
    [SerializeField]
    private Transform _bottom;
    [SerializeField]
    private float _passLineOffset;

    private EntityPool _pool;
    private int _stage;
    private float _passLine;
    private float _speed;

    public void Construct(HurdleData hurdleData, EntityPool pool)
    {
        Data = hurdleData;
        _pool = pool;
    }

    public void Set(int stage, float basePassLine, float maxHeight, float gap, float speed)
    {
        AdjustGap(maxHeight, gap);
        _stage = stage;
        _passLine = basePassLine - _passLineOffset;
        _speed = speed;
    }

    protected override void OnDespawn()
    {
        HasCollided = false;
        _pool.ReleaseHurdle(this);
    }

    protected override void OnMove(ref Vector2 velocity)
    {
        velocity = Vector2.left * _speed;
    }

    protected override void OnCollision(EntityBase entity)
    {
        if (HasCollided)
            return;

        if (entity.TryGetComponent(out PlayerCharacter pc))
        {
            float damage = pc.GetStat(StatType.MaxHp).Value;
            if (pc.CurrentLevel > 1)
                damage = damage * Data.Common.DamagePercent + (_stage / Data.Common.PowerUpStageInterval + 1) * Data.Common.BonusDamage;
                
            HitContext damageCtx = HitContext.Impact(damage, DamageFlags.TrueDamage | DamageFlags.IgnoreInvincible, null, pc);
            if (CombatSystem.Instance.TryApplyHit(damageCtx, out var result))
            {
                if (result.IsResolved)
                {
                    HasCollided = true;
                }
            }
        }
    }

    private void Update()
    {
        if (!IsSpawned || HasCollided)
            return;
        
        if (Position.x < _passLine)
        {
            HasCollided = true;
            Passed?.Invoke(this);
        }
    }

    private void AdjustGap(float maxHeight, float gap)
    {
        float t = 1f -  gap;
        float x = Random.Range(0f, t);

        float topPosition = maxHeight * (1f - x);
        float bottomPosition = maxHeight * (t - x - 1f);
        _top.localPosition = new Vector3(0f, topPosition, 0f);
        _bottom.localPosition = new Vector3(0f, bottomPosition, 0f);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!IsSpawned)
            return;

        Color c = Gizmos.color;
        Gizmos.color = Color.gainsboro;
        
        float halfHeight = Camera.main.orthographicSize;
        Vector2 min = new(Position.x + _passLineOffset, -halfHeight);
        Vector2 max = new(Position.x + _passLineOffset, halfHeight);
        Gizmos.DrawLine(min, max);

        Gizmos.color = c;
    }
#endif

    public HurdleData Data { get; private set; }
    public bool HasCollided { get; private set; }

    public static event Action<Hurdle> Passed;
}