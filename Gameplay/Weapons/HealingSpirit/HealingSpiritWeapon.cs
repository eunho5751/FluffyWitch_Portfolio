using System.Collections.Generic;

public class HealingSpiritWeapon : WeaponBase
{
    private readonly List<HealingSpirit> _spirits = new();
    private Stat _hpRegen;

    protected override void OnConsturct()
    {
        _hpRegen = Owner.GetStat(StatType.HpRegen);
        CreateSpirit();
    }

    protected override void OnEquip()
    {
        _hpRegen?.AddModifier(new StatModifier(WeaponData.HpRegen, StatModifierType.FlatAdd, this));
        SpawnSpirits();
    }

    protected override void OnUnequip()
    {
        _hpRegen?.RemoveAllModifiersFromSource(this);
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

    private void SpawnSpirit(HealingSpirit spirit)
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

    public new HealingSpiritWeaponData WeaponData => (HealingSpiritWeaponData)base.WeaponData;
}
