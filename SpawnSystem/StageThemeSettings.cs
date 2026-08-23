using UnityEngine;
using Sirenix.OdinInspector;

// ※ 발췌본 — 스폰 튜닝값과 로컬 스테이지 계산만 남기고, 테마 목록 열거 인터페이스
//    (IReadOnlyList 구현부) 등은 생략했다.
[HideMonoScript]
[CreateAssetMenu(menuName = "FluffyWitch/Stages/ThemeSettings")]
public class StageThemeSettings : ScriptableObject
{
    [Tooltip("구매된 몬스터를 내보내는 속도의 여유 배수. 예: 1.5면 몬스터가 구매되는 속도보다 1.5배 빠르게 스폰되어 평소엔 대기열이 쌓이지 않는다. 1에 가까울수록 스폰이 구매를 겨우 따라가 대기열이 생기기 쉽다.")]
    [SerializeField, MinValue(1f)]
    private float _spawnDrainMargin = 1.5f;
    [Tooltip("한 프레임에 스폰되는 최대 마릿수. 몬스터가 한꺼번에 많이 풀리는 순간에도 이 개수씩 나눠서 나온다 (순간 렉 방지). 평소 스폰 속도에는 영향이 없다.")]
    [SerializeField, MinValue(1)]
    private int _maxSpawnPerFrame = 8;

    [Space(5)]

    [SerializeField]
    private StageThemeData[] _themes;

    public int GetThemeIndexByStage(int stage)
    {
        int maxStage = 0;
        for (int i = 0; i < _themes.Length; i++)
        {
            maxStage += _themes[i].MaxStage;
            if (stage <= maxStage)
                return i;
        }
        return -1;
    }

    // 전역 스테이지 번호 → 테마 내 로컬 스테이지 (크레딧·가중치 커브의 X축 입력)
    public int GetLocalStage(int stage)
    {
        int themeIndex = GetThemeIndexByStage(stage);
        int localStage = stage;
        for (int i = 0; i < themeIndex; i++)
            localStage -= _themes[i].MaxStage;
        return localStage;
    }

    // … (발췌) 테마 열거·마지막 스테이지 판정 등 생략 …

    public StageThemeData this[int index] => _themes[index];
    public int Count => _themes.Length;
    public float SpawnDrainMargin => _spawnDrainMargin;
    public int MaxSpawnPerFrame => _maxSpawnPerFrame;
}
