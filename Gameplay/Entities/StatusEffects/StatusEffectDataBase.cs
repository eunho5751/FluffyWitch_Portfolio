using UnityEngine;
using Sirenix.OdinInspector;

[HideMonoScript]
public abstract class StatusEffectDataBase : ScriptableObject
{
    [SerializeField, MinValue(0)]
    private int _maxStacks = 1;
    [SerializeField, MinValue(0f)]
    private float _duration;
    [SerializeField]
    private StatusEffectDurationPolicy _durationPolicy = StatusEffectDurationPolicy.Refresh;
    [SerializeField, PropertyOrder(float.MaxValue)]
    private VFXClip _activeVFX;

    public abstract StatusEffectBase CreateInstance();

    public int MaxStacks => _maxStacks;
    public float Duration => _duration;
    public StatusEffectDurationPolicy DurationPolicy => _durationPolicy;
    public bool CanStack => _maxStacks > 1;
    public VFXClip ActiveVFX => _activeVFX;
}