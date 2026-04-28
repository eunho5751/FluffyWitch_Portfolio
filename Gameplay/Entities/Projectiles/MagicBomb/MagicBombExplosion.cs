using System.Collections.Generic;
using UnityEngine;

public class MagicBombExplosion : MonoBehaviour
{
    private static readonly List<Collider2D> ExplosionResults = new(64);

#if UNITY_EDITOR
    private float _radiusForGizmos;
#endif

    public void Execute(CharacterBase attacker, float damage, float radius)
    {
        ContactFilter2D filter = new();
        filter.SetLayerMask(EntityLayerMask.Get(attacker.IsPlayerCharacter ? EntityLayerFlags.Enemy : EntityLayerFlags.PlayerCharacter));
        filter.useTriggers = true;

        int cnt = Physics2D.OverlapCircle(transform.position, radius, filter, ExplosionResults);
        for (int i = 0; i < cnt; i++)
        {
            if (ExplosionResults[i].TryGetComponent(out CharacterBase target))
            {
                HitContext ctx = HitContext.Impact(damage, DamageFlags.None, attacker, target);
                CombatSystem.Instance.TryApplyHit(ctx, out _);
            }
        }

        transform.localScale = Vector3.one * radius;

#if UNITY_EDITOR
        _radiusForGizmos = radius;
#endif
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Color c = Gizmos.color;
        Gizmos.color = Color.lavender;
        Gizmos.DrawWireSphere(transform.position, _radiusForGizmos);
        Gizmos.color = c;
    }
#endif
}