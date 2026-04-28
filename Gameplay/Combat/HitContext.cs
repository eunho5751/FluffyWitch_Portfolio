using UnityEngine;

public readonly struct HitContext
{
    private HitContext(float damage, DamageKind damageKind, DamageFlags damageFlags, CharacterBase attacker, CharacterBase target, Vector2? hitPoint = null)
    {
        Damage = damage;
        DamageKind = damageKind;
        DamageFlags = damageFlags;
        Attacker = attacker;
        Target = target;
        HitPoint = hitPoint ?? Target.Position;
    }

    public static HitContext Impact(float damage, DamageFlags damageFlags, CharacterBase attacker, CharacterBase target, Vector2? hitPoint = null)
    {
        return new(damage, DamageKind.Impact, damageFlags, attacker, target, hitPoint);
    }

    public static HitContext DoT(float damage, CharacterBase attacker, CharacterBase target, Vector2? hitPoint = null)
    {
        return new(damage, DamageKind.DoT, DamageFlags.IgnoreInvincible | DamageFlags.TrueDamage, attacker, target, hitPoint);
    }

    public readonly float Damage { get; }
    public readonly DamageKind DamageKind { get; }
    public readonly DamageFlags DamageFlags { get; }
    public readonly CharacterBase Attacker { get; }
    public readonly CharacterBase Target { get; }
    public readonly Vector2 HitPoint { get; }
}