using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class CombatSystem
{
    private static readonly Lazy<CombatSystem> _instance = new();

    public bool TryApplyHit(HitContext ctx, out HitResult result)
    {
        if (!ctx.Target.IsAlive)
        {
            result = default;
            return false;
        }

        result = ResolveHit(ctx);
        ctx.Target.TakeHit(result);
        HitApplied?.Invoke(result);

        if (!ctx.Target.IsAlive)
        {
            HandleKill(ctx.Attacker, ctx.Target);
        }

        return true;
    }

    private HitResult ResolveHit(HitContext ctx)
    {
        var flags = ctx.DamageFlags;

        bool isImmune = ctx.Target.Conditions.Has(CharacterCondition.Invincible);
        if (isImmune && !flags.HasFlag(DamageFlags.IgnoreInvincible))
            return new HitResult(ctx, HitOutcome.Immune, 0f);

        bool isEvaded = !flags.HasFlag(DamageFlags.TrueDamage) && ctx.Target.IsPlayerCharacter && Random.value <= ctx.Target.GetStat(StatType.EvadeChance).Value;
        if (isEvaded)
            return new HitResult(ctx, HitOutcome.Miss, 0f);

        float damage = ResolveDamage(ctx, out bool isCritical);
        HitOutcome outcome = isCritical ? HitOutcome.CriticalHit : HitOutcome.Hit;
        return new HitResult(ctx, outcome, damage);
    }

    private float ResolveDamage(HitContext ctx, out bool isCritical)
    {
        isCritical = false;
        bool isTrueDamage = ctx.DamageFlags.HasFlag(DamageFlags.TrueDamage);
        float damage = ctx.Damage;
        if (ctx.Attacker != null && !isTrueDamage)
        {
            damage = ApplyAttackerStats(ctx.Attacker, damage, out isCritical);
            damage = ApplyTargetStats(ctx.Target, damage);
        }
        return damage;
    }

    private float ApplyAttackerStats(CharacterBase attacker, float damage, out bool isCritical)
    {
        isCritical = false;
        damage += attacker.GetStat(StatType.AttackDamage).Value;

        if (attacker.IsPlayerCharacter)
        {
            damage *= 1f + attacker.GetStat(StatType.DamageMultiplier).Value;
            isCritical = Random.value <= attacker.GetStat(StatType.CriticalChance).Value;
            if (isCritical)
                damage *= attacker.GetStat(StatType.CriticalHit).Value;
        }

        return damage;
    }

    private float ApplyTargetStats(CharacterBase target, float damage)
    {
        if (target.IsPlayerCharacter)
        {
            damage = Mathf.Max(1f, damage - target.GetStat(StatType.Armor).Value);
            damage = Mathf.Max(1f, damage * (1f - target.GetStat(StatType.DamageReduction).Value));
        }

        return damage;
    }

    private void HandleKill(CharacterBase attacker, CharacterBase victim)
    {
        if (attacker is PlayerCharacter playerCharacter)
        {
            ApplyLifesteal(playerCharacter);
        }

        Killed?.Invoke(attacker, victim);
    }

    private void ApplyLifesteal(PlayerCharacter playerCharacter)
    {
        float lifestealChance = playerCharacter.GetStat(StatType.LifestealChance).Value;
        if (Random.value <= lifestealChance)
        {
            Vector2 lifestealAmount = playerCharacter.Data.Common.LifestealAmount;
            playerCharacter.CurrentHp += Random.Range(lifestealAmount.x, lifestealAmount.y);
        }
    }

    public event Action<HitResult> HitApplied;
    public event Action<CharacterBase, CharacterBase> Killed;

    public static CombatSystem Instance => _instance.Value;
}