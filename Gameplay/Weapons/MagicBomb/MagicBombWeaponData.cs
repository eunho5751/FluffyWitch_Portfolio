using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "FluffyWitch/Weapons/MagicBomb")]
public class MagicBombWeaponData : WeaponDataBase
{
    [TitleGroup("Base")]
    [SerializeField, Required, AssetSelector]
    private MagicBomb _projectilePrefab;

    [TitleGroup("Weapon Attributes")]
    [SerializeField, MinValue(0f)]
    private float _attackCooldown = 2f;
    [SerializeField, MinValue(0f)]
    private float _attackOffset = 0.5f;
    

    [TitleGroup("Projectile Attributes")]
    [SerializeField, InlineProperty, HideLabel]
    private MagicBomb.Data _projectileData;

    public MagicBomb ProjectilePrefab => _projectilePrefab;
    public MagicBomb.Data ProjectileData => _projectileData;
    public float AttackCooldown => _attackCooldown;
    public float AttackOffset => _attackOffset;
}
