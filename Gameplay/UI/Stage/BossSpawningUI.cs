using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using Cysharp.Threading.Tasks;

public class BossSpawningUI : MonoBehaviour
{
    [SerializeField, Required]
    private Image _invasionTimeBar;

    private CancellationTokenSource _bossSpawningCTS;

    public void Initialize()
    {
    }

    public void Show()
    {
        if (gameObject.activeSelf)
            return;
        gameObject.SetActive(true);

        _bossSpawningCTS = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
        UpdateBossSpawning(StageManager.Instance.BossSpawner, _bossSpawningCTS.Token).Forget();         
    }

    public void Hide()
    {
        if (!gameObject.activeSelf)
            return;

        if (_bossSpawningCTS != null)
        {
            _bossSpawningCTS.Cancel();
            _bossSpawningCTS.Dispose();
            _bossSpawningCTS = null;
        }
        gameObject.SetActive(false);
    }

    private async UniTaskVoid UpdateBossSpawning(BossSpawner bossSpawner, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            UpdateInvasionTime(bossSpawner.RemainingTime, bossSpawner.SpawnInterval);       
            await UniTask.Yield(token);
        }
    }

    private void UpdateInvasionTime(float remainingTime, float maxTime)
    {
        _invasionTimeBar.fillAmount = (maxTime - remainingTime) / maxTime;
    }
}