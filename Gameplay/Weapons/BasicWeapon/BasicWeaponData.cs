using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "FluffyWitch/Weapons/BasicWeapon")]
public class BasicWeaponData : WeaponDataBase
{
    [TitleGroup("Base")]
    [SerializeField, Required, AssetSelector]
    private BasicMissile _projectilePrefab;

    [TitleGroup("Weapon Attributes")]
    [SerializeField, MinValue(0)]
    private int _maxUpgrade = 15;
    [SerializeField, MinValue(0)]
    private int _upgradeLevelInterval = 5;
    [SerializeField, MinValue(0)]
    private int _projectileCountPerUpgrade = 1;
    [SerializeField, MinValue(0f)]
    private float _projectileAngle = 5f;
    [SerializeField, MinValue(0f)]
    private float _attackCooldown = 0.3f;
    [SerializeField, MinValue(0f)]
    private float _attackOffset = 0.5f;

    [TitleGroup("Projectile Attributes")]
    [SerializeField, InlineProperty, HideLabel]
    private BasicMissile.Data _projectileData;

    public BasicMissile ProjectilePrefab => _projectilePrefab;
    public BasicMissile.Data ProjectileData => _projectileData;
    public int MaxUpgrade => _maxUpgrade;
    public int UpgradeLevelInterval => _upgradeLevelInterval;
    public int ProjectileCountPerUpgrade => _projectileCountPerUpgrade;
    public float ProjectileAngle => _projectileAngle;
    public float AttackCooldown => _attackCooldown;
    public float AttackOffset => _attackOffset;
}
