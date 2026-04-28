using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "FluffyWitch/Weapons/MagicShield")]
public class MagicShieldWeaponData : WeaponDataBase
{
    [TitleGroup("Base")]
    [SerializeField, Required, AssetSelector]
    private MagicShield _shieldPrefab;

    [TitleGroup("Attributes")]
    [SerializeField, MinValue(0f)]
    private float _damage = 1f;
    [SerializeField, MinValue(1)]
    private int _maxDurability = 5;
    [SerializeField, MinValue(0f)]
    private float _baseRadius = 1f;
    [SerializeField, MinValue(0f)]
    private float _radiusIncrementPerShield = 0.5f;
    [SerializeField, MinValue(0f)]
    private float _respawnDelay = 3f;

    public MagicShield ShieldPrefab => _shieldPrefab;
    public float Damage => _damage;
    public int MaxDurability => _maxDurability;
    public float BaseRadius => _baseRadius;
    public float RadiusIncrementPerShield => _radiusIncrementPerShield;
    public float RespawnDelay => _respawnDelay;
}
