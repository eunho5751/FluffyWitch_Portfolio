using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

[HideMonoScript]
[CreateAssetMenu(menuName = "FluffyWitch/PlayerCharacters/Common", fileName = "PlayerCharacterCommonData")]
public class PlayerCharacterCommonData : SerializedScriptableObject
{
    [TitleGroup("Level")]
    [SerializeField, MinValue(1)]
    private int _maxLevel = 20;
    [SerializeField, MinValue(1)]
    private int _initialLevelUpExp = 200;
    [SerializeField, MinValue(1f)]
    private float _levelUpExpMultiplier = 1.25f;

    [TitleGroup("Stats")]
    [SerializeField, DisableContextMenu, LabelText("Stat Definitions"),
    DictionaryDrawerSettings(IsReadOnly = true, KeyLabel = "Stat Type", ValueLabel = "Definition")]
    private Dictionary<StatType, StatDefinition> _statDefsMap = new();

    [Space(5)]

    [SerializeField, MinValue(0f)]
    private float _hpRegenInterval = 3f;
    [SerializeField, MinMaxSlider(0f, 10f, showFields: true)]
    private Vector2 _lifestealAmount = new(1f, 3f);

    [TitleGroup("Jump")]
    [SerializeField, MaxValue(0f)]
    private float _gravity = -9.81f * 2f;
    [SerializeField, MinValue(0f)]
    private float _jumpForce = 9f;
    [SerializeField, MinValue(0f)]
    private float _fallMultiplier = 1.5f;

    [TitleGroup("Hover")]
    [SerializeField, MinValue(0f)]
    private float _hoverThreshold = 0.2f;
    [SerializeField, MinValue(0f)]
    private float _hoverDuration = 0.6f;

#if UNITY_EDITOR
    private static readonly StatType[] RequiredStatTypes =
    {
        // Character Common
        StatType.MaxHp,
        StatType.AttackDamage,
        StatType.AttackSpeed,

        // PlayerCharacter
        StatType.HpRegen,
        StatType.CriticalChance,
        StatType.CriticalHit,
        StatType.Armor,
        StatType.DamageMultiplier,
        StatType.DamageReduction,
        StatType.EvadeChance,
        StatType.LifestealChance,
        StatType.GravityScale,
        StatType.HitboxScale,
        StatType.ItemMagnetRadius,
        StatType.ItemDropChance,
        StatType.ExpGainMultiplier,
        StatType.MpGainMultiplier,
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
            EditorUtility.SetDirty(this);
    }

    [TitleGroup("Stats"), Button]
    private void SyncStatsToPlayerCharacters()
    {
        int count = 0;
        var playerGuids = AssetDatabase.FindAssets("t:PlayerCharacterData");
        foreach (var guid in playerGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var data = AssetDatabase.LoadAssetAtPath<PlayerCharacterData>(path);
            if (data == null) continue;

            data.RefreshStats();
            count++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[FluffyWitch] Refreshed stats for {count} asset(s).");
    }
#endif

    public int MaxLevel => _maxLevel;
    public int InitialLevelUpExp => _initialLevelUpExp;
    public float LevelUpExpMultiplier => _levelUpExpMultiplier;

    public IReadOnlyDictionary<StatType, StatDefinition> StatDefsMap => _statDefsMap;
    public float HpRegenInterval => _hpRegenInterval;
    public Vector2 LifestealAmount => _lifestealAmount;

    public float Gravity => _gravity;
    public float JumpForce => _jumpForce;
    public float FallMultiplier => _fallMultiplier;

    public float HoverThreshold => _hoverThreshold;
    public float HoverDuration => _hoverDuration;
}