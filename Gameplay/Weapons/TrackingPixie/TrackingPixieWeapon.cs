using System.Collections.Generic;

public class TrackingPixieWeapon : WeaponBase
{
    private readonly List<TrackingPixie> _spirits = new();

    protected override void OnConsturct()
    {
        CreateSpirit();
    }

    protected override void OnEquip()
    {
        SpawnSpirits();
    }

    protected override void OnUnequip()
    {
        DespawnSpirits();
    }

    private void CreateSpirit()
    {
        var spirit = Instantiate(WeaponData.SpiritPrefab, transform);
        spirit.Construct(this);
        spirit.gameObject.SetActive(false);
        _spirits.Add(spirit);

        if (IsEquipped)
            SpawnSpirit(spirit);
    }

    private void SpawnSpirit(TrackingPixie spirit)
    {
        spirit.gameObject.SetActive(true);
        spirit.Launch();
        Owner.SpiritController.Register(spirit.transform);
    }

    private void SpawnSpirits()
    {
        foreach (var spirit in _spirits)
        {
            SpawnSpirit(spirit);
        }
    }

    private void DespawnSpirits()
    {
        foreach (var spirit in _spirits)
        {
            Owner.SpiritController.Unregister(spirit.transform);
            spirit.gameObject.SetActive(false);
        }
    }

    public new TrackingPixieWeaponData WeaponData => (TrackingPixieWeaponData)base.WeaponData;
}
