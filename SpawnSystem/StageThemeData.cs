using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

// ※ 발췌본 — 크레딧 스폰과 무관한 필드(표시 이름, 테마 프리팹, 허들·보스 스폰 룰)와
//    관련 메서드는 생략했다. 테마 = "스테이지 여러 개를 묶는 단위"이며, 커브의 X축은
//    테마 내 로컬 스테이지다.
[HideMonoScript]
[CreateAssetMenu(menuName = "FluffyWitch/Stages/ThemeData")]
public class StageThemeData : ScriptableObject
{
    [SerializeField, MinValue(1)]
    private int _maxStage = 1;

    [Space(5)]

    // X = 테마 내 로컬 스테이지, Y = 초당 크레딧 수급량
    [SerializeField]
    private AnimationCurve _creditRateCurve = AnimationCurve.Constant(1f, 99f, 1f);
    [SerializeField, HideReferenceObjectPicker]
    private EnemyRosterEntry[] _enemyRoster;

    // … (발췌) 허들·보스 스폰 룰 배열과 구간 룰 조회 메서드 생략 …

    public int MaxStage => _maxStage;
    public IReadOnlyList<EnemyRosterEntry> EnemyRoster => _enemyRoster;
    public AnimationCurve CreditRateCurve => _creditRateCurve;
}
