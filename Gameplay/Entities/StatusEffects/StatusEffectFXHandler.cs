using System.Collections.Generic;
using UnityEngine;

public class StatusEffectFXHandler
{
    private readonly Dictionary<StatusEffectBase, GameObject> _vfxMap = new();
    private readonly StatusEffectHandler _handler;

    public StatusEffectFXHandler(StatusEffectHandler handler)
    {
        _handler = handler;
        handler.Applied += OnApplied;
        handler.Removed += OnRemoved;
    }

    public void Dispose()
    {
        _handler.Applied -= OnApplied;
        _handler.Removed -= OnRemoved;
    }

    private void OnApplied(StatusEffectBase effect)
    {
        var vfx = effect.Data.ActiveVFX;
        if (!vfx.IsValid)
            return;

        var vfxInstance = vfx.Play(Vector2.zero);
        vfxInstance.transform.SetParent(_handler.Owner.transform, false);
        _vfxMap.Add(effect, vfxInstance);
    }

    private void OnRemoved(StatusEffectBase effect)
    {
        if (_vfxMap.Remove(effect, out var vfx))
        {
            VFXManager.Instance.Despawn(vfx);
        }
    }
}