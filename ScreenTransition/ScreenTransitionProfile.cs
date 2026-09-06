using System;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Utilities;

[CreateAssetMenu(menuName = "Screen Transition/Screen Transition Profile"), InlineEditor]
public class ScreenTransitionProfile : ScriptableObject
{
    [SerializeField]
    private Vector2 _referenceResolution = new(1920f, 1080f);
#if UNITY_EDITOR
    [OnValueChanged(nameof(OnEffectPrefabChanged))]
#endif
    [SerializeField, AssetSelector, AssetsOnly]
    private ScreenTransitionEffectBase _effectPrefab;
    [SerializeReference, BoxGroup,
        HideReferenceObjectPicker, DisableContextMenu, HideIf("@_effectPrefab == null")]
    private object _effectSettings;

    public T GetEffectSettings<T>()
    {
        return (T) _effectSettings;
    }

#if UNITY_EDITOR
    private void OnEnable()
    {
        OnEffectPrefabChanged(_effectPrefab);
    }

    private void OnEffectPrefabChanged(ScreenTransitionEffectBase prefab)
    {
        if (prefab == null)
        {
            _effectSettings = null;
            return;
        }
        
        Type settingType = null;
        var type = prefab.GetType();
        if (type.ImplementsOpenGenericClass(typeof(ScreenTransitionEffectBase<>)))
        {
            settingType = type.GetArgumentsOfInheritedOpenGenericClass(typeof(ScreenTransitionEffectBase<>))[0];
        }

        bool settingsExist = _effectSettings != null && settingType == _effectSettings.GetType();
        if (!settingsExist)
        {
            if (settingType != null)
            {
                _effectSettings = Activator.CreateInstance(settingType);
            }
            else
            {
                _effectPrefab = null;
                _effectSettings = null;
            }
        }
    }
#endif

    public Vector2 ReferenceResolution => _referenceResolution;
    public ScreenTransitionEffectBase EffectPrefab => _effectPrefab;
}