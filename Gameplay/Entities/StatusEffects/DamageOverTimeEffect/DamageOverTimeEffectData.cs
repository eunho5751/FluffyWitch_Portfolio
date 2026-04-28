using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "FluffyWitch/StatusEffects/DamageOverTime")]
public class DamageOverTimeEffectData : StatusEffectDataBase
{
    [SerializeField, MinValue(0f)]
    private float _damagePerTick;
    [SerializeField, MinValue(0f)]
    private float _tickInterval;

    public override StatusEffectBase CreateInstance() => new DamageOverTimeEffect(this);

    public float DamagePerTick => _damagePerTick;
    public float TickInterval => _tickInterval;
}