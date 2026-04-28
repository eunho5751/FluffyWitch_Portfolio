using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "FluffyWitch/Weapons/HealingSpirit")]
public class HealingSpiritWeaponData : WeaponDataBase
{
    [TitleGroup("Base")]
    [SerializeField, Required, AssetSelector]
    private HealingSpirit _spiritPrefab;
    [SerializeField, Required, AssetSelector]
    private SpiritMissile _projectilePrefab;

    [TitleGroup("Weapon Attributes")]
    [SerializeField, MinValue(0f)]
    private float _aimRange = 4f;
    [SerializeField, MinValue(0f)]
    private float _attackCooldown = 1.5f;
    [SerializeField, MinValue(0f)]
    private float _hpRegen = 1f;

    [TitleGroup("Projectile Attributes")]
    [SerializeField, InlineProperty, HideLabel]
    private SpiritMissile.Data _projectileData;

    public HealingSpirit SpiritPrefab => _spiritPrefab;
    public SpiritMissile ProjectilePrefab => _projectilePrefab;
    public SpiritMissile.Data ProjectileData => _projectileData;
    public float AimRange => _aimRange;
    public float AttackCooldown => _attackCooldown;
    public float HpRegen => _hpRegen;
}
