using System;
using UnityEngine;
using Sirenix.OdinInspector;

public class ChainThunderbolt : ProjectileBase<ChainThunderbolt.Data>
{
    [Serializable]
    public struct Data
    {
        [MinValue(0f)] public float Damage;
        [MinValue(0f)] public float Speed;
        [Required, AssetSelector] public ChainLightning ChainLightningPrefab;
        [MinValue(0)] public int MaxChains;
        [MinValue(0f)] public float ChainDamageMultiplier;
        [MinValue(0f)] public float ChainRange;
    }

    protected override void OnMove(ref Vector2 velocity)
    {
        velocity = CalculateVelocity(Direction, ProjectileData.Speed);
    }

    protected override void OnCollision(EntityBase entity)
    {
        float projectileDamage = ProjectileData.Damage;
        if (TryApplyHit(entity, projectileDamage, out var result))
        {
            if (result.IsResolved)
            {
                PerformChain(entity);
                Despawn();
            }
        }
    }

    private void PerformChain(EntityBase firstTarget)
    {
        int maxChains = ProjectileData.MaxChains;
        float chainRange = ProjectileData.ChainRange;
        float damage = ProjectileData.Damage * ProjectileData.ChainDamageMultiplier;

        var chainLightning = PoolManager.Instance.Get<ChainLightning>(ProjectileData.ChainLightningPrefab.gameObject);
        chainLightning.gameObject.SetActive(true);
        chainLightning.Execute(Owner, firstTarget, maxChains, chainRange, damage);
    }
}
