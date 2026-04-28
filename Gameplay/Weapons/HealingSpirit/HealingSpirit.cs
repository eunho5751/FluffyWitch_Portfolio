using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class HealingSpirit : MonoBehaviour
{
    private HealingSpiritWeapon _weapon;

    public void Construct(HealingSpiritWeapon weapon)
    {
        _weapon = weapon;
    }

    public void Launch()
    {
        FireLoop(_weapon.UnequipToken).Forget();
    }

    private async UniTaskVoid FireLoop(CancellationToken token)
    {
        var weaponData = _weapon.WeaponData;
        var pool = StageManager.Instance.Entities;
        while (!token.IsCancellationRequested)
        {
            Vector2 position = transform.position;
            Vector2 direction = _weapon.Direction;
            var closestOpponent = EntityHelper.FindClosestOpponent(_weapon.Owner.IsPlayerCharacter, transform.position, 0f, weaponData.AimRange);
            if (closestOpponent != null)
            {
                direction = closestOpponent.Position - position;
                direction.Normalize();
            }

            var missile = pool.GetProjectile(weaponData.ProjectilePrefab);
            missile.Set(_weapon.Owner, direction, weaponData.ProjectileData);
            missile.Spawn(transform.position);

            float cooldown = _weapon.CalculateCooldown(weaponData.AttackCooldown);
            await UniTask.WaitForSeconds(cooldown, cancellationToken: token);
        }
    }
}
