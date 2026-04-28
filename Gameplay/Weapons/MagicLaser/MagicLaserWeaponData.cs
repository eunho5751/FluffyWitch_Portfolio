using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "FluffyWitch/Weapons/MagicLaser")]
public class MagicLaserWeaponData : WeaponDataBase
{
    [TitleGroup("Base")]
    [SerializeField, Required, AssetSelector]
    private MagicLaser _laserPrefab;
    
    [TitleGroup("Attributes")]
    [SerializeField, MinValue(0f)]
    private float _damage = 1f;
    [SerializeField, MinValue(0f)]
    private float _hitInterval = 0.1f;
    [SerializeField, MinValue(0f)]
    private Vector2 _beamScale = Vector2.one;
    [SerializeField, MinValue(1)]
    private float _duration = 1f;
    [SerializeField, MinValue(0f)]
    private float _attackCooldown = 1f;
    [SerializeField, MinValue(0f)]
    private float _attackOffset = 0.5f;

    public MagicLaser LaserPrefab => _laserPrefab;
    public float Damage => _damage;
    public float HitInterval => _hitInterval;
    public Vector2 BeamScale => _beamScale;
    public float Duration => _duration;
    public float AttackCoolodwn => _attackCooldown;
    public float AttackOffset => _attackOffset;
}
