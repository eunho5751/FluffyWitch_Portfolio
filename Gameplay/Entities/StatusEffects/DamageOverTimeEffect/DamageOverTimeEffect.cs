using UnityEngine;

public class DamageOverTimeEffect : StatusEffectBase<DamageOverTimeEffectData>
{
    private float _tickTimer;

    public DamageOverTimeEffect(DamageOverTimeEffectData data) : base(data)
    {
    }

    public override void OnApply()
    {
        _tickTimer = 0f;
    }

    public override void OnTick()
    {
        float tickInterval = Data.TickInterval;
        if (tickInterval <= 0f)
            return;

        _tickTimer += Time.deltaTime;
        while (_tickTimer >= tickInterval)
        {
            _tickTimer -= tickInterval;
            ApplyDamage();
        }
    }

    private void ApplyDamage()
    {
        float damage = Data.DamagePerTick * Stacks;
        HitContext ctx = HitContext.DoT(damage, Caster, Target);
        CombatSystem.Instance.TryApplyHit(ctx, out _);
    }
}