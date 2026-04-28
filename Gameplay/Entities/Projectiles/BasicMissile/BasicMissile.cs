using System;
using UnityEngine;
using Sirenix.OdinInspector;

public class BasicMissile : ProjectileBase<BasicMissile.Data>
{
    [Serializable]
    public struct Data
    {
        [MinValue(0f)] public float Damage;
        [MinValue(0f)] public float Speed;
    }

    protected override void OnMove(ref Vector2 velocity)
    {
        velocity = CalculateVelocity(Direction, ProjectileData.Speed);
    }

    protected override void OnCollision(EntityBase entity)
    {
        TryApplyHit(entity, ProjectileData.Damage, out _);
    }
}
