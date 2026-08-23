using System;
using UnityEngine;
using Sirenix.OdinInspector;

[Serializable]
public class EnemyRosterEntry
{
    [InlineProperty, HideReferenceObjectPicker]
    [SerializeField, Required]
    private GuidRef<EnemyData> _enemy;
    [SerializeField, MinValue(1)]
    private int _cost = 1;
    // X = 테마 내 로컬 스테이지, Y = 스폰 추첨 가중치 (0 이하는 제외)
    [SerializeField]
    private AnimationCurve _weightCurve = AnimationCurve.Constant(1f, 99f, 1f);

    public string Guid => _enemy.Guid;
    public int Cost => _cost;
    public AnimationCurve WeightCurve => _weightCurve;
}
