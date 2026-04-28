using System;
using UnityEngine;
using Sirenix.OdinInspector;

public class MagicBomb : ProjectileBase<MagicBomb.Data>
{
    [Serializable]
    public struct Data
    {
        [MinValue(0f)] public float Damage;
        [MinValue(0f)] public float Speed;
        [Required, AssetSelector] public MagicBombExplosion ExplosionPrefab;
        [MinValue(0f)] public float ExplosionRadius;
        [MinValue(0f)] public float ExplosionDamageMultiplier;
    }

    private CharacterBase _target;

    protected override void OnSpawn()
    {
        _target = EntityHelper.FindClosestOpponent(Owner.IsPlayerCharacter, Position, 0f, float.MaxValue);
    }

    protected override void OnMove(ref Vector2 velocity)
    {
        if (_target != null && _target.IsSpawned)
        {
            velocity = CalculateTrackingVelocity(_target, ProjectileData.Speed);
        }
        else
        {
            velocity = CalculateVelocity(Direction, ProjectileData.Speed);
        }
    }

    protected override void OnCollision(EntityBase entity)
    {
        if (TryApplyHit(entity, ProjectileData.Damage, out var result))
        {
            if (result.IsResolved)
            {
                Explode();
            }
        }
    }

    private void Update()
    {
        var bounds = Bounds;
        if (StageManager.Instance.IsOverStageBounds(bounds.min, bounds.max))
        {
            Explode();
        }
    }

    private void Explode()
    {
        float damage = ProjectileData.Damage * ProjectileData.ExplosionDamageMultiplier;
        float radius = ProjectileData.ExplosionRadius;

        var explosion = PoolManager.Instance.Get<MagicBombExplosion>(ProjectileData.ExplosionPrefab.gameObject);
        explosion.transform.position = Position;
        explosion.gameObject.SetActive(true);
        explosion.Execute(Owner, damage, radius);

        Despawn();
    }
}
