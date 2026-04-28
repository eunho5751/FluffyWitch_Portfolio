using UnityEngine;
using Sirenix.OdinInspector;

public abstract class WeaponDataBase : GuidScriptableObjectBase
{
    [TitleGroup("Base")]
    [SerializeField, Required]
    private string _displayName;    
    [TitleGroup("Base")]
    [SerializeField, Required, AssetSelector]
    private WeaponBase _weaponPrefab;

    public string DisplayName => _displayName;
    public WeaponBase WeaponPrefab => _weaponPrefab;
}