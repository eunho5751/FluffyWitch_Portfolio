using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "FluffyWitch/StatusEffects/Registry")]
public class StatusEffectRegistry : ScriptableObject
{
    private static StatusEffectRegistry _instance = null;

    [SerializeField, Required]
    private ConditionEffectData _hitInvincibility;
    [SerializeField, Required]
    private DamageOverTimeEffectData _burning;

    private static StatusEffectRegistry Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<StatusEffectRegistry>(nameof(StatusEffectRegistry));
                if (_instance == null)
                    Debug.LogError($"Failed to find {nameof(StatusEffectRegistry)} instance.");
            }
            return _instance;
        }
    }

    public static ConditionEffectData HitInvincibility => Instance._hitInvincibility;
    public static DamageOverTimeEffectData Burning => Instance._burning;
}