using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class BasicWeapon : WeaponBase
{
    protected override void OnEquip()
    {
        if (Owner is not PlayerCharacter pc)
            return;

        Launch(pc, UnequipToken).Forget();
    }

    private async UniTaskVoid Launch(PlayerCharacter pc, CancellationToken token)
    {
        var pool = StageManager.Instance.Entities;
        var weaponData = (BasicWeaponData)WeaponData;
        while (!token.IsCancellationRequested)
        {
            int count = Mathf.Min((1 + pc.CurrentLevel / weaponData.UpgradeLevelInterval) * weaponData.ProjectileCountPerUpgrade, weaponData.MaxUpgrade * weaponData.ProjectileCountPerUpgrade);
            float totalAngle = (count - 1) * weaponData.ProjectileAngle;
            float startAngle = -totalAngle * 0.5f;

            Vector2 spawnPosition = pc.Position + Direction * weaponData.AttackOffset;
            for (int i = 0; i < count; i++)
            {
                float angle = startAngle + i * weaponData.ProjectileAngle;
                Vector2 dir = (Quaternion.Euler(0f, 0f, angle) * Direction).normalized;

                var missile = pool.GetProjectile(weaponData.ProjectilePrefab);
                missile.Set(Owner, dir, weaponData.ProjectileData);
                missile.Spawn(spawnPosition);
            }

            float cooldown = CalculateCooldown(weaponData.AttackCooldown);
            await UniTask.WaitForSeconds(cooldown, cancellationToken: token);
        }
    }
}