using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "FluffyWitch/Weapons/ChainThunderbolt")]
public class ChainThunderboltWeaponData : WeaponDataBase
{
    [TitleGroup("Base")]
    [SerializeField, Required, AssetSelector]
    private ChainThunderbolt _projectilePrefab;

    [TitleGroup("Weapon Attributes")]
    [SerializeField, MinValue(0f)]
    private float _attakCooldown = 2f;
    [SerializeField, MinValue(0f)]
    private float _attackOffset = 0.5f;

    [TitleGroup("Projectile Attributes")]
    [SerializeField, InlineProperty, HideLabel]
    private ChainThunderbolt.Data _projectileData;

    public ChainThunderbolt ProjectilePrefab => _projectilePrefab;
    public ChainThunderbolt.Data ProjectileData => _projectileData;
    public float AttackCooldown => _attakCooldown;
    public float AttackOffset => _attackOffset;
}
