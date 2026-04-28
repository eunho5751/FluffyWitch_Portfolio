using System;
using UnityEngine;
using Sirenix.OdinInspector;

[Serializable, InlineProperty]
public struct VFXClip
{
    [SerializeField, HideLabel]
    private GuidRef<VFXData> _vfx;

    public GameObject Play(Vector2 position) => IsValid ? VFXManager.Instance.Spawn(_vfx, position) : null;

    public readonly bool IsValid => _vfx != null && _vfx.IsValid;
}