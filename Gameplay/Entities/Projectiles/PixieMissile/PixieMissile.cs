using System;
using UnityEngine;
using Sirenix.OdinInspector;

public class PixieMissile : ProjectileBase<PixieMissile.Data>
{
    [Serializable]
    public struct Data
    {
        [MinValue(0f)] public float Damage;
        [MinValue(0f)] public float Speed;
        [MinMaxSlider(0f, 100f, ShowFields = true)] public Vector2 TrackingRange;
        [MinValue(0f)] public float InertiaDuration;
        [MinValue(0f)] public float Lifetime;
    }

    private enum Mode
    {
        Tracking,
        Inertia
    }

    private Mode _mode;
    private float _inertiaTimer;
    private float _lifetimeTimer;
    private Vector2 _moveDirection;
    private EntityBase _target;

    protected override void OnSpawn()
    {
        _mode = Mode.Tracking;
        _inertiaTimer = 0f;
        _lifetimeTimer = 0f;
        _moveDirection = Direction;
        _target = null;
    }

    protected override void OnMove(ref Vector2 velocity)
    {
        _lifetimeTimer += Time.fixedDeltaTime;
        if (_lifetimeTimer >= ProjectileData.Lifetime)
        {
            Despawn();
            return;
        }

        switch (_mode)
        {
            case Mode.Tracking:
                UpdateTracking(ref velocity);
                break;
            case Mode.Inertia:
                UpdateInertia(ref velocity);
                break;
        }
    }

    private void UpdateTracking(ref Vector2 velocity)
    {
        if (_target == null || !_target.IsSpawned)
            _target = EntityHelper.FindClosestOpponent(Owner.IsPlayerCharacter, PhysicsPosition, ProjectileData.TrackingRange.x, ProjectileData.TrackingRange.y);

        if (_target != null)
        {
            velocity = CalculateTrackingVelocity(_target, ProjectileData.Speed);
            _moveDirection = velocity.normalized;
        }
        else
        {
            velocity = CalculateVelocity(_moveDirection, ProjectileData.Speed);
            _moveDirection = velocity.normalized;
        }
    }

    private void UpdateInertia(ref Vector2 velocity)
    {
        _inertiaTimer += Time.fixedDeltaTime;
        if (_inertiaTimer >= ProjectileData.InertiaDuration)
        {
            _mode = Mode.Tracking;
            _target = null;
        }

        velocity = CalculateVelocity(_moveDirection, ProjectileData.Speed);
    }

    protected override void OnCollision(EntityBase entity)
    {
        if (TryApplyHit(entity, ProjectileData.Damage, out var result, shouldDespawnOnHitResolved: false))
        {
            if (result.IsResolved)
            {
                _mode = Mode.Inertia;
                _inertiaTimer = 0f;
                _target = null;

                if (result.IsHit)
                {
                    var target = result.Target;
                    target.StatusEffects.Apply(StatusEffectRegistry.Burning);
                }
            }
        }
    }
}
