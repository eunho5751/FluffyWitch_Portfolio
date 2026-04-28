using System;
using System.Threading;
using UnityEngine;
using Sirenix.OdinInspector;
using Cysharp.Threading.Tasks;

[CreateAssetMenu(menuName = "FluffyWitch/ScriptableActions/Boundary")]
public class BoundaryAction : ScriptableActionBase
{
    [SerializeField]
    private bool _instantKill;
    [SerializeField, HideIfGroup("NoKill", Condition = "$_instantKill"), Range(0f, 1f)]
    private float _percentDamage = 0.9f;
    [SerializeField, HideIfGroup("NoKill")]
    private float _knockbackForce;
    [SerializeField, HideIfGroup("NoKill"), MinValue(0f)]
    private float _jumpLockDuration = 0.1f;

    private CancellationTokenSource _cts;

    protected override void OnInvoke()
    {
        if (!Target.TryGetComponent(out PlayerCharacter playerCharacter))
            return;

        float percent = _percentDamage;
        DamageFlags damageFlags = DamageFlags.TrueDamage;
        if (_instantKill)
        {
            percent = 1f;
            damageFlags |= DamageFlags.IgnoreInvincible;
        }
        float damage = playerCharacter.GetStat(StatType.MaxHp).Value * percent;
        HitContext ctx = HitContext.Impact(damage, damageFlags, null, playerCharacter);
        if (CombatSystem.Instance.TryApplyHit(ctx, out var result))
        {
            if (playerCharacter.IsAlive)
            {
                playerCharacter.Velocity = Vector2.up * _knockbackForce;

                if (_jumpLockDuration > 0f)
                {
                    if (_cts != null)
                    {
                        _cts.Cancel();
                        _cts.Dispose();
                    }
                    _cts = new();
                    UniTask.Void(async token =>
                    {
                        try
                        {
                            playerCharacter.Conditions.Add(CharacterCondition.JumpLock); 
                            await UniTask.WaitForSeconds(_jumpLockDuration, cancellationToken: token);
                        }
                        finally
                        {
                            playerCharacter.Conditions.Remove(CharacterCondition.JumpLock);
                        }
                    }, _cts.Token);
                }
            }
        }
    }
}