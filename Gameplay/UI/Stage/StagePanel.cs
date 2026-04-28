using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using Cysharp.Threading.Tasks;

public class StagePanel : MonoBehaviour
{
    [Title("Stage")]
    [SerializeField]
    private TextMeshProUGUI _stageText;

    [Title("Player Character")]
    [SerializeField, Required]
    private PlayerCharacterStatusUI _playerCharacterStatus;

    [Title("Boss")]
    [SerializeField, Required]
    private BossSpawningUI _bossSpawning;
    [SerializeField, Required]
    private BossStatusUI _bossStatus;

    private Enemy _bossCache;

    public void Initialize()
    {
        _playerCharacterStatus.Initialize();
        _bossSpawning.Initialize();
        _bossStatus.Initialize();

        // Stage
        StageManager.Instance.StageChanged += OnStageChanged;
        OnStageChanged(StageManager.Instance.CurrentStage);

        // Boss
        var bossSpawner = StageManager.Instance.BossSpawner;

        bossSpawner.SpawningStateChanged += OnBossSpawningStateChanged;
        OnBossSpawningStateChanged(bossSpawner.IsStarted);

        bossSpawner.Spawned += OnBossSpawned;
        var currentBoss = StageManager.Instance.CurrentBoss;
        if (currentBoss != null)
            OnBossSpawned(currentBoss);
    }

    public void Dispose()
    {
        if (!StageManager.HasInstance)
            return;

        StageManager.Instance.StageChanged -= OnStageChanged;

        var bossSpawner = StageManager.Instance.BossSpawner;
        bossSpawner.SpawningStateChanged -= OnBossSpawningStateChanged;
        bossSpawner.Spawned -= OnBossSpawned;
    }

    private void UpdateStage(int stage)
    {
        _stageText.text = stage.ToString();
    }

    private void OnStageChanged(int currentStage)
    {
        UpdateStage(currentStage);
    }

#region Boss
    private void OnBossSpawningStateChanged(bool started)
    {
        if (started)
            _bossSpawning.Show();
        else
            _bossSpawning.Hide();
    }

    private void OnBossSpawned(Enemy boss)
    {
        boss.Died += OnBossDied;
        _bossCache = boss;
        _bossStatus.Show(boss);
    }

    private void OnBossDied()
    {
        _bossCache.Died -= OnBossDied;
        _bossCache = null;
        _bossStatus.Hide();
    }
#endregion
}