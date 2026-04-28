using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "FluffyWitch/Weapons/CometSpear")]
public class CometSpearWeaponData : WeaponDataBase
{
    [TitleGroup("Base")]
    [SerializeField, Required, AssetSelector]
    private CometSpear _projectilePrefab;

    [TitleGroup("Weapon Attributes")]
    [SerializeField, MinValue(0f)]
    private float _attackCooldown = 1.5f;
    [SerializeField, MinValue(0f)]
    private float _attackOffset = 0.5f;

    [TitleGroup("Projectile Attributes")]
    [SerializeField, InlineProperty, HideLabel]
    private CometSpear.Data _projectileData;

    public CometSpear ProjectilePrefab => _projectilePrefab;
    public CometSpear.Data ProjectileData => _projectileData;
    public float AttackCooldown => _attackCooldown;
    public float AttackOffset => _attackOffset;
}
