using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class ChainThunderboltWeapon : WeaponBase
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
            Vector2 direction = Direction;
            var closestOpponent = EntityHelper.FindClosestOpponent(Owner.IsPlayerCharacter, Owner.Position, 0f, float.MaxValue);
            if (closestOpponent != null)
            {
                direction = closestOpponent.Position - Owner.Position;
                direction.Normalize();
            }

            Vector2 spawnPosition = Owner.Position + Direction * WeaponData.AttackOffset;
            var lightning = pool.GetProjectile(WeaponData.ProjectilePrefab);
            lightning.Set(Owner, direction, WeaponData.ProjectileData);
            lightning.Spawn(spawnPosition);

            float cooldown = CalculateCooldown(WeaponData.AttackCooldown);
            await UniTask.WaitForSeconds(cooldown, cancellationToken: token);
        }
    }

    public new ChainThunderboltWeaponData WeaponData => (ChainThunderboltWeaponData)base.WeaponData;
}
