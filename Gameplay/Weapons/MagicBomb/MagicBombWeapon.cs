using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class MagicBombWeapon : WeaponBase
{
    protected override void OnEquip()
    {
        Launch(UnequipToken).Forget();
    }

    private async UniTaskVoid Launch(CancellationToken token)
    {
        var pool = StageManager.Instance.Entities;
        while (!token.IsCancellationRequested)
        {
            Vector2 spawnPosition = Owner.Position + Direction * WeaponData.AttackOffset;
            var bomb = pool.GetProjectile(WeaponData.ProjectilePrefab);
            bomb.Set(Owner, Direction, WeaponData.ProjectileData);
            bomb.Spawn(spawnPosition);

            float cooldown = CalculateCooldown(WeaponData.AttackCooldown);
            await UniTask.WaitForSeconds(cooldown, cancellationToken: token);
        }
    }

    public new MagicBombWeaponData WeaponData => (MagicBombWeaponData)base.WeaponData;
}
