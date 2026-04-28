using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;


#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "FluffyWitch/PlayerCharacters/PlayerCharacter")]
public class PlayerCharacterData : GuidScriptableObjectBase
{
    [SerializeField, DisableIf("@_commonData != null")]
    private PlayerCharacterCommonData _commonData;

    [TitleGroup("Base")]
    [SerializeField, Required]
    private string _displayName;
    [SerializeField, Required, AssetSelector]
    private PlayerCharacter _prefab;

    [TitleGroup("Attributes")]
    [SerializeField, DisableContextMenu, LabelText("Stats"),
    DictionaryDrawerSettings(IsReadOnly = true, KeyLabel = "Stat Type", ValueLabel = "Base Value")]
    private Dictionary<StatType, float> _statsMap = new();

    [Space(5)]

    [SerializeField, Required]
    private GuidRef<WeaponDataBase>[] _initialWeapons;

    public KeyValuePair<StatType, Stat>[] CreateStats()
    {
        int idx = 0;
        KeyValuePair<StatType, Stat>[] stats = new KeyValuePair<StatType, Stat>[_statsMap.Count];
        foreach (var kvp in _statsMap)
        {
            var def = _commonData.StatDefsMap[kvp.Key];
            Stat stat = new(kvp.Value, def.MinValue, def.MaxValue);
            stats[idx] = new(kvp.Key, stat);
            idx++;
        }
        return stats;
    }

#if UNITY_EDITOR
    [OnInspectorInit]
    private void OnInspectorInit()
    {
        RefreshStats();
    }

    internal void RefreshStats()
    {
        if (_commonData == null) return;

        bool dirty = false;

        var keys = _statsMap.Keys.ToArray();
        foreach (var type in keys)
        {
            if (!_commonData.StatDefsMap.ContainsKey(type))
            {
                _statsMap.Remove(type);
                dirty = true;
            }
        }

        foreach (var kvp in _commonData.StatDefsMap)
        {
            if (!_statsMap.ContainsKey(kvp.Key))
            {
                _statsMap.Add(kvp.Key, 0f);
                dirty = true;
            }
        }

        if (dirty)
            EditorUtility.SetDirty(this);
    }

    protected override void OnValidate()
    {
        base.OnValidate();
        
        if (_commonData == null)
        {
            string dataPath = $"Assets/_Project/Data/Entities/{nameof(PlayerCharacterCommonData)}.asset";
            _commonData = AssetDatabase.LoadAssetAtPath<PlayerCharacterCommonData>(dataPath);

            if (_commonData == null)
                Debug.LogError($"Can't find {nameof(PlayerCharacterCommonData)}.asset in {System.IO.Path.GetDirectoryName(dataPath)}.");
        }
    }
#endif

    public PlayerCharacterCommonData Common => _commonData;

    public string DisplayName => _displayName;
    public PlayerCharacter Prefab => _prefab;

    public IReadOnlyList<string> InitialWeapons => Array.ConvertAll(_initialWeapons, static w => (string)w);
}
