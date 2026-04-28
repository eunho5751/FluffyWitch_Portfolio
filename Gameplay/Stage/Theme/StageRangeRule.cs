using System;
using UnityEngine;
using Sirenix.OdinInspector;

[Serializable]
public class StageRangeRule<T>
{
    [PropertySpace(SpaceBefore = 5)]
    [Title("$StageRangeLabel", bold: true)]

    [SerializeField, MinValue(1), MaxValue("$MaxStageRange")]
    private int _stageRange = 1;
    [TitleGroup("Spawn Rule", GroupID = "SpawnRule")]
    [SerializeField, HideLabel, HideDuplicateReferenceBox]
    private T _rule;

    public int StageRange => _stageRange;
    public T Rule => _rule;

#if UNITY_EDITOR
    [PropertySpace(SpaceBefore = 5)]
    [HorizontalGroup("SpawnRule/Buttons", marginLeft: 5, marginRight: 5)]
    [Button("Copy")]
    private void Copy() { _copiedJson = JsonUtility.ToJson(_rule); }
    [PropertySpace(SpaceBefore = 5)]
    [HorizontalGroup("SpawnRule/Buttons")]
    [Button("Paste"), EnableIf("$_copiedJson")]
    private void Paste() => JsonUtility.FromJsonOverwrite(_copiedJson, _rule);
#endif

#if UNITY_EDITOR
    private int _startStage;
    private int _maxStage;
    private static string _copiedJson;

    private string StageRangeLabel => $"STAGE : {_startStage} ~ {_startStage + _stageRange - 1} ({_stageRange})";
    private int MaxStageRange => _maxStage - _startStage + 1;

    internal void SetEditorContext(int startStage, int maxStage)
    {
        _startStage = startStage;
        _maxStage = maxStage;
    }
#endif
}
