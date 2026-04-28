using System.Collections.Generic;

public class MagicShieldWeapon : WeaponBase
{
    private readonly List<MagicShield> _shields = new();

    protected override void OnConsturct()
    {
        CreateShield();
    }

    protected override void OnEquip()
    {
        SpawnShields();
    }

    protected override void OnUnequip()
    {
        DespawnShields();
    }

    private void CreateShield()
    {
        float radius = WeaponData.BaseRadius + _shields.Count * WeaponData.RadiusIncrementPerShield;
        var shield = Instantiate(WeaponData.ShieldPrefab, transform);
        shield.Construct(this, radius);
        shield.gameObject.SetActive(false);
        _shields.Add(shield);

        if (IsEquipped)
            SpawnShield(shield);
    }

    private void SpawnShield(MagicShield shield)
    {
        shield.gameObject.SetActive(true);
    }

    private void SpawnShields()
    {
        foreach (var shield in _shields)
        {
            SpawnShield(shield);
        }
    }

    private void DespawnShields()
    {
        foreach (var shield in _shields)
        {
            shield.gameObject.SetActive(false);
        }
    }

    public new MagicShieldWeaponData WeaponData => (MagicShieldWeaponData)base.WeaponData;
}
