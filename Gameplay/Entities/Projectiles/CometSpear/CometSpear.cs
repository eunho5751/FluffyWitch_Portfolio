using System;
using UnityEngine;
using Sirenix.OdinInspector;

public class CometSpear : ProjectileBase<CometSpear.Data>
{
    [Serializable]
    public struct Data
    {
        [MinValue(0f)] public float Damage;
        [MinValue(0f)] public float DamageIncreaseRate;
        [MinValue(0f)] public float Speed;
    }

    private int _hitCount;

    protected override void OnSpawn()
    {
        _hitCount = 0;
    }

    protected override void OnMove(ref Vector2 velocity)
    {
        velocity = CalculateVelocity(Direction, ProjectileData.Speed);
    }

    protected override void OnRotate(ref float angle)
    {
        angle = Mathf.Atan2(Direction.y, Direction.x) * Mathf.Rad2Deg;
    }

    protected override void OnCollision(EntityBase entity)
    {
        float damage = ProjectileData.Damage * (1f + ProjectileData.DamageIncreaseRate * _hitCount);
        if (TryApplyHit(entity, damage, out var result))
        {
            if (result.IsHit)
            {
                _hitCount++;
            }
        }
    }
}
