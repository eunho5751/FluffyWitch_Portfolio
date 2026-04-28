using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class MagicShield : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D _rigidbody;
    [SerializeField]
    private ArcShieldMesh _shieldMesh;

    private MagicShieldWeapon _weapon;
    private int _currentDurability;
    private CancellationTokenSource _cts;

    public void Construct(MagicShieldWeapon weapon, float radius)
    {
        _weapon = weapon;
        _rigidbody.excludeLayers = 1 << weapon.Owner.Layer;
        _shieldMesh.SetMaxDurability(weapon.WeaponData.MaxDurability);
        _shieldMesh.SetRadius(radius);
        Respawn();
    }

    private void Respawn()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;

        CurrentDurability = _weapon.WeaponData.MaxDurability;
    }

    private async UniTaskVoid ChargeAndRespawn(CancellationToken token)
    {
        await UniTask.WaitForSeconds(_weapon.WeaponData.RespawnDelay, cancellationToken: token);
        Respawn();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_cts != null)
            return;

        if (collision.TryGetComponent(out EntityBase entity))
        {
            bool hit = false;
            if (entity is CharacterBase target)
            {
                float damage = _weapon.WeaponData.Damage;
                HitContext ctx = HitContext.Impact(damage, DamageFlags.None, _weapon.Owner, target);
                if (CombatSystem.Instance.TryApplyHit(ctx, out var result))
                {
                    hit = result.IsResolved;
                }
            }
            else if (entity is ProjectileBase proj && proj.Owner != _weapon.Owner)
            {
                entity.Despawn();
                hit = true;
            }

            if (hit)
            {
                CurrentDurability--;
                if (CurrentDurability <= 0)
                {
                    _cts = CancellationTokenSource.CreateLinkedTokenSource(_weapon.UnequipToken);
                    ChargeAndRespawn(_cts.Token).Forget();
                }
            }
        }
    }

    private int CurrentDurability
    {
        get
        {
            return _currentDurability;
        }

        set
        {
            _currentDurability = Mathf.Max(0, value);
            _shieldMesh.SetDurability(_currentDurability);
        }
    }
}
