using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;


#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "FluffyWitch/Enemies/Enemy")]
public class EnemyData : GuidScriptableObjectBase
{
    [SerializeField, DisableIf("@_commonData != null")]
    private EnemyCommonData _commonData;

    [TitleGroup("Base")]
    [SerializeField, Required]
    private string _displayName;
    [SerializeField, Required, AssetSelector]
    private Enemy _prefab;

    [TitleGroup("Attributes")]
    [SerializeField, DisableContextMenu, LabelText("Stats"),
    DictionaryDrawerSettings(IsReadOnly = true, KeyLabel = "Stat Type", ValueLabel = "Base Value")]
    private Dictionary<StatType, float> _statsMap = new();
    [SerializeField, DisableContextMenu, LabelText("Stat Growths Per Stage"),
    DictionaryDrawerSettings(IsReadOnly = true, KeyLabel = "Stat Type", ValueLabel = "Growth Value")]
    private Dictionary<StatType, float> _statGrowthsMap = new();

    [Space(5)]

    [SerializeField, Required]
    private GuidRef<WeaponDataBase>[] _initialWeapons;
    [SerializeField, Required]
    private MovePatternBase _movePattern;

    [TitleGroup("Rewards")]
    [SerializeField, MinValue(0)]
    private int _rewardExp;
    [SerializeField, MinValue(0f)]
    private float _rewardExpBonusPerStage;

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
        RefreshStats(_statsMap);
        RefreshStats(_statGrowthsMap);
    }

    private void RefreshStats(Dictionary<StatType, float> map)
    {
        if (_commonData == null) return;

        bool dirty = false;

        var keys = map.Keys.ToArray();
        foreach (var type in keys)
        {
            if (!_commonData.StatDefsMap.ContainsKey(type))
            {
                map.Remove(type);
                dirty = true;
            }
        }

        foreach (var kvp in _commonData.StatDefsMap)
        {
            if (!map.ContainsKey(kvp.Key))
            {
                map.Add(kvp.Key, 0f);
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
            string dataPath = $"Assets/_Project/Data/Entities/{nameof(EnemyCommonData)}.asset";
            _commonData = AssetDatabase.LoadAssetAtPath<EnemyCommonData>(dataPath);

            if (_commonData == null)
                Debug.LogError($"Can't find {nameof(EnemyCommonData)}.asset in {System.IO.Path.GetDirectoryName(dataPath)}.");
        }
    }
#endif

    public EnemyCommonData Common => _commonData;

    public string DisplayName => _displayName;
    public Enemy Prefab => _prefab;

    public IEnumerable<KeyValuePair<StatType, float>> StatGrowthsPerStage => _statGrowthsMap;
    public IReadOnlyList<string> InitialWeapons => Array.ConvertAll(_initialWeapons, static w => (string)w);
    public MovePatternBase MovePattern => _movePattern;

    public int RewardExp => _rewardExp;
    public float RewardExpBonusPerStage => _rewardExpBonusPerStage;
}