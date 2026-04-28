using UnityEngine;

public class MagicLaserWeapon : WeaponBase
{
    private MagicLaser _laser;

    protected override void OnConsturct()
    {
        _laser = Instantiate(WeaponData.LaserPrefab, transform);
        _laser.Construct(this);
    }

    protected override void OnEquip()
    {
        _laser.StartLaunch();
    }

    protected override void OnUnequip()
    {
        _laser.StopLaunch();
    }

    public new MagicLaserWeaponData WeaponData => (MagicLaserWeaponData)base.WeaponData;
}