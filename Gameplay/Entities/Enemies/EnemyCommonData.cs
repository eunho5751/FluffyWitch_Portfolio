using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

[HideMonoScript]
[CreateAssetMenu(menuName = "FluffyWitch/Enemies/Common", fileName = "EnemyCommonData")]
public class EnemyCommonData : SerializedScriptableObject
{
    [TitleGroup("Stats")]
    [SerializeField, DisableContextMenu, LabelText("Stat Definitions"),
    DictionaryDrawerSettings(IsReadOnly = true, KeyLabel = "Stat Type", ValueLabel = "Definition")]
    private Dictionary<StatType, StatDefinition> _statDefsMap = new();
    
    #if UNITY_EDITOR
    private static readonly StatType[] RequiredStatTypes =
    {
        // Character Common
        StatType.MaxHp,
        StatType.AttackDamage,
        StatType.AttackSpeed,

        // Enemy
        StatType.CollisionDamage,
        StatType.MoveSpeed
    };

    [OnInspectorInit]
    private void OnInspectorInit()
    {
        RefreshStatDefinitions();
    }

    private void RefreshStatDefinitions()
    {
        bool dirty = false;

        var keys = _statDefsMap.Keys.ToArray();
        foreach (var type in keys)
        {
            bool exists = Array.Exists(RequiredStatTypes, x => x == type);
            if (!exists)
            {
                _statDefsMap.Remove(type);
                dirty = true;
            }
        }

        for (int i = 0; i < RequiredStatTypes.Length; i++)
        {
            StatType statType = RequiredStatTypes[i];
            if (!_statDefsMap.ContainsKey(statType))
            {
                _statDefsMap.Add(statType, new StatDefinition());
                dirty = true;
            }
        }

        if (dirty)
            UnityEditor.EditorUtility.SetDirty(this);
    }

    [TitleGroup("Stats"), Button]
    private void SyncStatsToEnemies()
    {
        int count = 0;
        var enemyGuids = AssetDatabase.FindAssets("t:EnemyData");
        foreach (var guid in enemyGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var data = AssetDatabase.LoadAssetAtPath<EnemyData>(path);
            if (data == null) continue;

            data.RefreshStats();
            count++;
        }
        
        AssetDatabase.SaveAssets();
        Debug.Log($"[FluffyWitch] Refreshed stats for {count} asset(s).");
    }
#endif

    public IReadOnlyDictionary<StatType, StatDefinition> StatDefsMap => _statDefsMap;
}