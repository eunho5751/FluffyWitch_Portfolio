using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;
using Cysharp.Threading.Tasks;

[DefaultExecutionOrder(int.MinValue + 1)]
public class StageManager : SingletonBehaviourBase<StageManager>
{
    [Title("Base")]
    [SerializeField, LabelText("Viewport Start Point")]
    private Vector2 _vpStartPoint = new(0.15f, 0.5f);
    [SerializeField, LabelText("Viewport Despawn Lines Padding"), MinValue(0f)]
    private Vector2 _vpDespawnLinesPadding = new(0.15f, 0.2f);

    [Space(10)]

    [Title("References")]
    [SerializeField, Required]
    private StageConfig _stageConfig;
    [SerializeField, Required]
    private EntityPool _entityPool;
    [SerializeField, Required]
    private EnemySpawner _enemySpawner;
    [SerializeField, Required]
    private HurdleSpawner _hurdleSpawner;
    [SerializeField, Required]
    private BossSpawner _bossSpawner;

    private StageThemeCollection _themeCollection;
    private StageThemeSettings _currentTheme;
    private int _currentThemeIndex = -1;
    private int _currentEnemyRangeRuleIndex = -1;
    private int _currentHurdleRangeRuleIndex = -1;
    private int _currentBossRangeRuleIndex = -1;

    private CancellationTokenSource _stateCTS;
    private bool _isStateTransitioning;
    
    private InputAction _pauseAction;
    private InputAction _resumeAction;
    private InputAction _restartAction;

    private Rect _despawnLines;

    public void Initialize()
    {
        _themeCollection = StageConfig.ThemeCollection;
        if (_themeCollection.Count == 0)
        {
            Debug.LogError("There must be at least one theme in StageThemeCollection!");
            return;
        }

        MaxStage = _themeCollection.TotalStage;
        SpawnPlayerCharacter(StageConfig.DefaultPlayerCharacter);
        InitializeInputs();
        InitializeDespawnLines();
        InitializeSpawners();
        SetStage(1);

        ChangeState(StageState.Initialized);
    }

    public void Dispose()
    {
        _enemySpawner.Dispose();
        _hurdleSpawner.Dispose();
        _bossSpawner.Dispose();
    }

    public void Resume(bool immediate)
    {
        if (State != StageState.Paused)
            return;
        State = immediate ? StageState.Playing : StageState.Resuming;
    }

    public void Pause()
    {
        if (State == StageState.Paused)
            return;
        State = StageState.Paused;
    }

    public void Restart()
    {
        GameManager.Instance.MoveToStage();
    }

    public bool TryAdvanceStage()
    {
        if (IsLastStage)
            return false;

        SetStage(CurrentStage + 1);
        return true;
    }

    public bool IsOverStageBounds(Vector2 boundsMin, Vector2 boundsMax)
    {
        var camera = Camera.main;
        Vector3 min = camera.WorldToViewportPoint(boundsMin);
        Vector3 max = camera.WorldToViewportPoint(boundsMax);
        return min.x < 0f || max.x > 1f || min.y < 0f || max.y > 1f;
    }

    public bool IsOutOfDespawnBounds(Vector2 position)
    {
        return !_despawnLines.Contains(position);
    }

    private void InitializeInputs()
    {
        var actions = InputSystem.actions;
        _pauseAction = actions.FindAction("Pause");
        _resumeAction = actions.FindAction("Resume");
        _restartAction = actions.FindAction("Restart");
    }

    private void InitializeDespawnLines()
    {
        var camera = Camera.main;
        var rectMin = camera.ViewportToWorldPoint(Vector2.zero - _vpDespawnLinesPadding);
        var rectMax = camera.ViewportToWorldPoint(Vector2.one + _vpDespawnLinesPadding);
        _despawnLines = new(rectMin, rectMax - rectMin);
    }

    private void InitializeSpawners()
    {
        _enemySpawner.Construct();
        _hurdleSpawner.Construct();
        _bossSpawner.Construct();
        _bossSpawner.Spawned += OnBossSpawned;
    }

    private void SpawnPlayerCharacter(PlayerCharacterData playerCharacterData)
    {
        var startPosition = Camera.main.ViewportToWorldPoint(_vpStartPoint);
        PlayerCharacter = Instantiate(playerCharacterData.Prefab);
        PlayerCharacter.Construct(playerCharacterData);
        PlayerCharacter.Spawn(startPosition);
        PlayerCharacter.Died += OnPlayerCharacterDied;
    }

    private void SetStage(int stage)
    {
        if (stage > MaxStage)
        {
            Debug.LogError($"Stage {stage} exceeds max stage!");
            return;
        }

        var themeIndex = _themeCollection.GetThemeIndexByStage(stage);
        if (themeIndex == -1)
        {
            Debug.LogError($"No theme found for stage {stage}!");
            return;
        }

        if (_currentThemeIndex != themeIndex)
            ChangeTheme(themeIndex);

        int localStage = _themeCollection.GetLocalStage(stage);
        UpdateRangeRule(_currentTheme.EnemySpawnRules, localStage, ref _currentEnemyRangeRuleIndex, ApplyEnemyRule);
        UpdateRangeRule(_currentTheme.HurdleSpawnRules, localStage, ref _currentHurdleRangeRuleIndex, ApplyHurdleRule);
        UpdateRangeRule(_currentTheme.BossSpawnRules, localStage, ref _currentBossRangeRuleIndex, ApplyBossRule, forceApply: true);

        CurrentStage = stage;
        StageChanged?.Invoke(CurrentStage);
    }

    private void ChangeTheme(int themeIndex)
    {
        _currentThemeIndex = themeIndex;
        _currentTheme = _themeCollection[_currentThemeIndex];
    }

    private void UpdateRangeRule<T>(IReadOnlyList<StageRangeRule<T>> rangeRules, int localStage, ref int currentIndex, Action<T> applyRule, bool forceApply = false) where T : class
    {
        int newIndex = _currentTheme.GetRangeRuleIndex(rangeRules, localStage);
        if (!forceApply && currentIndex == newIndex)
            return;

        currentIndex = newIndex;
        applyRule(currentIndex == - 1 ? null : rangeRules[currentIndex].Rule);
    }

    private void ApplyEnemyRule(EnemySpawnRule rule)
    {
        if (_enemySpawner.IsStarted)
            _enemySpawner.StopSpawning();
        if (rule != null)
            _enemySpawner.StartSpawning(rule);
    }

    private void ApplyHurdleRule(HurdleSpawnRule rule)
    {
        if (_hurdleSpawner.IsStarted)
            _hurdleSpawner.StopSpawning();
        if (rule != null)
            _hurdleSpawner.StartSpawning(rule);
    }

    private void ApplyBossRule(BossSpawnRule rule)
    {
        if (_bossSpawner.IsStarted)
            _bossSpawner.StopSpawning();
        if (rule != null && !IsBossSpawned)
            _bossSpawner.StartSpawning(rule, repeatSpawning: false);
    }

    private void ChangeState(StageState targetState)
    {
        if (State == targetState || _isStateTransitioning)
            return;
        
        _isStateTransitioning = true;
        {
            var prevState = State;
            _stateCTS?.Cancel();
            _stateCTS = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
            if (State != StageState.None)
                OnStateExit(prevState);
            State = targetState;
            OnStateEnter(State);
            OnStateUpdate(State, _stateCTS.Token).Forget();
            StateChanged?.Invoke(prevState, State);
        }
        _isStateTransitioning = false;
    }

#region Listeners
    private void OnPlayerCharacterDied()
    {
        ChangeState(StageState.FinishingStage);
    }

    private void OnBossSpawned(Enemy boss)
    {
        CurrentBoss = boss;
        CurrentBoss.Died += OnBossDied;
    }

    private void OnBossDied()
    {
        CurrentBoss.Died -= OnBossDied;
        CurrentBoss = null;

        TryAdvanceStage();
    }
#endregion

#region States
    private void OnStateEnter(StageState state)
    {
        switch (state)
        {
            case StageState.Initialized:
            {
                async UniTaskVoid ChangeToPlayingState(CancellationToken token)
                {
                    await UniTask.NextFrame(token);
                    ChangeState(StageState.Playing);
                }

                Time.timeScale = 0f;
                ChangeToPlayingState(_stateCTS.Token).Forget();
            }
            break;

            case StageState.Playing:
            {
                Time.timeScale = 1f;
                _pauseAction.performed += OnPauseInputPerformed;
                _resumeAction.Disable();
            }
            break;

            case StageState.FinishingStage:
            {
                async UniTaskVoid GameOverAfterDelay(CancellationToken token)
                {
                    await UniTask.WaitForSeconds(2f, ignoreTimeScale: true, cancellationToken: token);
                    ChangeState(StageState.GameOver);
                }

                Time.timeScale = 0f;
                GameOverAfterDelay(_stateCTS.Token).Forget();
            }
            break;

            case StageState.GameOver:
            {
                _restartAction.performed += OnRestartInputPerformed;
            }
            break;

            case StageState.Resuming:
            {
                async UniTaskVoid ResumeAfterDelay(CancellationToken token)
                {
                    await UniTask.WaitForSeconds(1f, ignoreTimeScale: true, cancellationToken: token);
                    ChangeState(StageState.Playing);
                }

                ResumeAfterDelay(_stateCTS.Token).Forget();
            }
            break;

            case StageState.Paused:
            {
                Time.timeScale = 0f;
                _resumeAction.performed += OnResumeInputPerformed;
                _resumeAction.Enable();
            }
            break;
        }
    }

    private async UniTaskVoid OnStateUpdate(StageState state, CancellationToken token)
    {
        await UniTask.NextFrame(token);
        while (!token.IsCancellationRequested)
        {
            await UniTask.Yield(token);
        }
    }

    private void OnStateExit(StageState state)
    {
        // DO NOT CHANGE STATE IN THIS METHOD !

        switch (state)
        {
            case StageState.Playing:
            {
                _pauseAction.performed -= OnPauseInputPerformed;
            }
            break;

            case StageState.Paused:
            {
                _resumeAction.performed -= OnResumeInputPerformed;
            }
            break;

            case StageState.GameOver:
            {
                _restartAction.performed -= OnRestartInputPerformed;
            }
            break;
        }
    }
#endregion

#region Inputs
    private void OnPauseInputPerformed(InputAction.CallbackContext ctx)
    {
        ChangeState(StageState.Paused);
    }

    private void OnResumeInputPerformed(InputAction.CallbackContext ctx)
    {
        ChangeState(StageState.Playing);
    }

    private void OnRestartInputPerformed(InputAction.CallbackContext ctx)
    {
        _restartAction.performed -= OnRestartInputPerformed;
        Restart();
    }
#endregion

#if UNITY_EDITOR
    private void DrawStartingPointGizmos(Camera camera)
    {
        var pos = camera.ViewportToWorldPoint(_vpStartPoint);
        Gizmos.DrawIcon(pos, "T_StartingPoint.png");
    }

    private void DrawDespawnLinesGizmos(Camera camera)
    {
        var c = Gizmos.color;
        Gizmos.color = Color.red;

        var lb = camera.ViewportToWorldPoint(Vector2.zero - _vpDespawnLinesPadding);
        var rt = camera.ViewportToWorldPoint(Vector2.one + _vpDespawnLinesPadding);
        var center = (rt + lb) * 0.5f;
        var size = rt - lb;
        Gizmos.DrawWireCube(center, size);

        Gizmos.color = c;
    }

    private void OnDrawGizmos()
    {
        var camera = Camera.main;
        if (camera == null)
            return;

        DrawStartingPointGizmos(camera);
        DrawDespawnLinesGizmos(camera);
    }
#endif

    public StageConfig StageConfig => _stageConfig;
    public StageState State { get; private set; } = StageState.None;

    public BossSpawner BossSpawner => _bossSpawner;
    public Enemy CurrentBoss { get; private set; }
    public bool IsBossSpawned => CurrentBoss != null;

    public bool IsLastStage => CurrentStage == MaxStage;
    public int MaxStage { get; private set; }
    public int CurrentStage { get; private set; }

    public PlayerCharacter PlayerCharacter { get; private set; }
    public EntityPool Entities => _entityPool;

    public event Action<StageState, StageState> StateChanged;
    public event Action<int> StageChanged;
}