using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class CometSpearWeapon : WeaponBase
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
            var frontTarget = EntityHelper.FindFrontOpponent(Owner.IsPlayerCharacter, Owner.Position, 0f, float.MaxValue);
            if (frontTarget != null)
            {
                direction = frontTarget.Position - Owner.Position;
                direction.Normalize();
            }
            
            Vector2 spawnPosition = Owner.Position + Direction * WeaponData.AttackOffset;
            Quaternion spawnRotation = Quaternion.FromToRotation(Vector3.right, direction);
            var spear = pool.GetProjectile(WeaponData.ProjectilePrefab);
            spear.Set(Owner, direction, WeaponData.ProjectileData);
            spear.Spawn(spawnPosition, spawnRotation);

            float cooldown = CalculateCooldown(WeaponData.AttackCooldown);
            await UniTask.WaitForSeconds(cooldown, cancellationToken: token);
        }
    }

    public new CometSpearWeaponData WeaponData => (CometSpearWeaponData)base.WeaponData;
}
