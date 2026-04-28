using System;
using UnityEngine;
using Sirenix.OdinInspector;

[Serializable]
public struct FXClip
{
    [SerializeField, LabelText("VFX")]
    private VFXClip _vfxClip;
    // [SerializeField]
    // private SFXClip _sfxClip;

    public readonly GameObject Play(Vector2 position)
    {
        // _sfxClip.Play();
        return _vfxClip.Play(position);
    }
}