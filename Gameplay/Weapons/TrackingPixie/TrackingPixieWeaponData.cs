using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "FluffyWitch/Weapons/TrackingPixie")]
public class TrackingPixieWeaponData : WeaponDataBase
{
    [TitleGroup("Base")]
    [SerializeField, Required, AssetSelector]
    private TrackingPixie _spiritPrefab;
    [SerializeField, Required, AssetSelector]
    private PixieMissile _projectilePrefab;

    [TitleGroup("Weapon Attributes")]
    [SerializeField, MinValue(0f)]
    private float _attackCooldown = 1.5f;
    
    [TitleGroup("Projectile Attributes")]
    [SerializeField, InlineProperty, HideLabel]
    private PixieMissile.Data _projectileData;

    public TrackingPixie SpiritPrefab => _spiritPrefab;
    public PixieMissile ProjectilePrefab => _projectilePrefab;
    public PixieMissile.Data ProjectileData => _projectileData;
    public float AttackCooldown => _attackCooldown;
}
