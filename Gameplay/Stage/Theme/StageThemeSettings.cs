using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[HideMonoScript]
[CreateAssetMenu(menuName = "FluffyWitch/Stages/ThemeSettings")]
public class StageThemeSettings : ScriptableObject
{
    [SerializeField, MinValue(1)]
    private int _maxStage = 1;

    [Space(5)]

    [SerializeField]
    private StageRangeRule<EnemySpawnRule>[] _enemySpawnRules;

    [Space(5)]

    [SerializeField]
    private StageRangeRule<HurdleSpawnRule>[] _hurdleSpawnRules;

    [Space(5)]

    [SerializeField]
    private StageRangeRule<BossSpawnRule>[] _bossSpawnRules;

    public int GetRangeRuleIndex<T>(IReadOnlyList<StageRangeRule<T>> rangeRules, int localStage)
    {
        int stageRange = 0;
        for (int i = 0; i < rangeRules.Count; i++)
        {
            stageRange += rangeRules[i].StageRange;
            if (localStage <= stageRange)
                return i;
        }
        return -1;
    }

    public int MaxStage => _maxStage;
    public IReadOnlyList<StageRangeRule<EnemySpawnRule>> EnemySpawnRules => _enemySpawnRules;
    public IReadOnlyList<StageRangeRule<HurdleSpawnRule>> HurdleSpawnRules => _hurdleSpawnRules;
    public IReadOnlyList<StageRangeRule<BossSpawnRule>> BossSpawnRules => _bossSpawnRules;

#if UNITY_EDITOR
    [OnInspectorInit]
    private void OnInspectorInit() => ResolveAll();
    private void OnValidate() => ResolveAll();

    private void ResolveAll()
    {
        ResolveEditorContext(_enemySpawnRules);
        ResolveEditorContext(_hurdleSpawnRules);
        ResolveEditorContext(_bossSpawnRules);
    }

    private void ResolveEditorContext<T>(StageRangeRule<T>[] rules)
    {
        if (rules == null)
            return;
            
        int startStage = 1;
        for (int i = 0; i < rules.Length; i++)
        {
            var rule = rules[i];
            rule.SetEditorContext(startStage, _maxStage);
            startStage += rule.StageRange;
        }
    }
#endif
}
