using UnityEngine;

public readonly struct HitResult
{
    private readonly HitContext _context;

    public HitResult(HitContext context, HitOutcome outcome, float damage)
    {
        _context = context;
        Outcome = outcome;
        Damage = damage;
    }

    public readonly HitOutcome Outcome { get; }
    public readonly bool IsHit => Outcome == HitOutcome.Hit || Outcome == HitOutcome.CriticalHit;
    public readonly bool IsResolved => Outcome != HitOutcome.Immune;
    public readonly float Damage { get; }

    public readonly DamageKind DamageKind => _context.DamageKind;
    public readonly CharacterBase Attacker => _context.Attacker;
    public readonly CharacterBase Target => _context.Target;
    public readonly Vector2 HitPoint => _context.HitPoint;
}