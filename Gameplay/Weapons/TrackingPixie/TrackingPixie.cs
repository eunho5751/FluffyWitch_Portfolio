using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class TrackingPixie : MonoBehaviour
{
    private TrackingPixieWeapon _weapon;

    public void Construct(TrackingPixieWeapon weapon)
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

            var missile = pool.GetProjectile(weaponData.ProjectilePrefab);
            missile.Set(_weapon.Owner, direction, weaponData.ProjectileData);
            missile.Spawn(position);

            float cooldown = _weapon.CalculateCooldown(weaponData.AttackCooldown);
            await UniTask.WaitForSeconds(cooldown, cancellationToken: token);
        }
    }
}
