using UnityEngine;
using Sirenix.OdinInspector;

// ※ StageManager.cs 발췌본 — 스폰 시스템의 배선과 수명주기(언제 무엇이 갱신되는가)에
//    해당하는 부분만 남겼다. 스테이지 상태 머신, 일시정지, 테마 클리어 연출, 입력 처리,
//    허들·보스 스포너 관련 코드는 생략했다.
public class StageManager : SingletonBehaviourBase<StageManager>
{
    [SerializeField, Required]
    private EntityPool _entityPool;
    [SerializeField, Required]
    private EnemySpawner _enemySpawner;

    private StageThemeSettings _themeSettings;
    private StageThemeData _currentTheme;
    private GameObject _currentThemeInstance;
    private int _currentThemeIndex = -1;

    private readonly EnemySpawnTable _enemySpawnTable = new();
    private readonly StageDifficulty _difficulty = new();

    // 씬 초기화 — 스포너에 스폰 테이블과 전역 튜닝값(방출 여유 배수·프레임당 상한)을 주입한다
    private void InitializeDependencies()
    {
        _enemySpawner.Construct(_enemySpawnTable, _themeSettings.SpawnDrainMargin, _themeSettings.MaxSpawnPerFrame);

        // … (발췌) 그 외 시스템 초기화 생략 …
    }

    private void SetStage(int stage)
    {
        // … (발췌) 스테이지 범위·테마 유효성 검사 생략 …

        var themeIndex = _themeSettings.GetThemeIndexByStage(stage);
        if (_currentThemeIndex != themeIndex)
            ChangeTheme(themeIndex);

        int localStage = _themeSettings.GetLocalStage(stage);
        // 스폰 가중치·크레딧 커브는 스테이지가 입력이라 구간 룰과 달리 매 스테이지 갱신한다
        _enemySpawnTable.ApplyStage(localStage);
        _enemySpawner.ApplyCreditRate(_currentTheme.CreditRateCurve.Evaluate(localStage));

        // … (발췌) 허들·보스 구간 룰 갱신 생략 …

        CurrentStage = stage;
    }

    private void ChangeTheme(int themeIndex)
    {
        _currentThemeIndex = themeIndex;
        _currentTheme = _themeSettings[_currentThemeIndex];

        if (_currentThemeInstance != null)
            Destroy(_currentThemeInstance);
        _currentThemeInstance = Instantiate(_currentTheme.ThemePrefab);

        // 테마가 바뀌면 로스터 구성 자체가 달라지므로 테이블을 재구축하고 크레딧·큐를 초기화한다
        _enemySpawnTable.Rebuild(_currentTheme, GameManager.Instance.Database);
        _enemySpawner.ResetState();
    }

    // 룰 적용과 스폰 시작은 분리되어 있다 — Playing이 아닐 때(씬 진입·테마 전환) 적용된
    // 설정은 보관만 해두고, Playing 상태 진입 시 여기서 시작한다.
    private void StartPendingSpawners()
    {
        if (!_enemySpawner.IsStarted)
            _enemySpawner.StartSpawning();

        // … (발췌) 허들·보스 스포너 시작 생략 …
    }

    // 플레이어 사망·테마 클리어 시퀀스 진입 시에는 _enemySpawner.StopSpawning()으로
    // 새 스폰만 즉시 차단한다 (기존 엔티티 목록은 건드리지 않아 히트 처리 콜스택에서도 안전).

    public int CurrentStage { get; private set; }
    public StageDifficulty Difficulty => _difficulty;
    public EntityPool Entities => _entityPool;
}
