using System;
using UnityEngine;
using UnityEngine.Events;

public abstract class ProjectileBase : EntityBase
{
    private EntityPool _pool;
    private bool _isConstructed;

    public void Construct(EntityPool pool)
    {
        if (_isConstructed)
            return;

        _pool = pool;
        _isConstructed = true;
    }

    protected override void OnDespawn()
    {
        _pool.ReleaseProjectile(this);
    }

    public CharacterBase Owner { get; protected set; }
}

public abstract class ProjectileBase<TData> : ProjectileBase where TData : struct
{
    [Space(10)]

    [SerializeField]
    private FXClip _hitFX;

    public void Set(CharacterBase owner, Vector2 direction, TData data)
    {
        Owner = owner;
        Direction = direction;
        ProjectileData = data;
        ExcludeCollisionLayers(1 << Owner.Layer);
    }

    protected bool TryApplyHit(EntityBase target, float damage, out HitResult result, DamageFlags damageFlags = DamageFlags.None, bool shouldDespawnOnHitResolved = true)
    {
        if (target is CharacterBase character)
        {
            HitContext damageCtx = HitContext.Impact(damage, damageFlags, Owner, character, Position);
            if (CombatSystem.Instance.TryApplyHit(damageCtx, out result))
            {
                if (result.IsResolved)
                {
                    _hitFX.Play(Position);

                    if (shouldDespawnOnHitResolved)
                    {
                        Despawn();
                    }
                }

                return true;
            }
        }

        result = default;
        return false;
    }

    protected Vector2 CalculateTrackingVelocity(EntityBase target, float speed)
    {
        Vector2 toTarget = target.PhysicsPosition - PhysicsPosition;
        toTarget.Normalize();
        if (Owner.IsPlayerCharacter)
        {
            bool isBehind = toTarget.x < 0f;
            speed = isBehind ? Mathf.Max(target.Velocity.magnitude * 1.5f, speed) : speed;
        }
        return toTarget * speed;
    }

    protected Vector2 CalculateVelocity(Vector2 targetDir, float speed)
    {
        if (Owner.IsPlayerCharacter)
        {
            bool isBehind = targetDir.x < 0f;
            if (isBehind)
                speed *= 1.5f;
        }
        return targetDir * speed;
    }

    protected Vector2 Direction { get; private set; }
    public TData ProjectileData { get; private set; }
}
