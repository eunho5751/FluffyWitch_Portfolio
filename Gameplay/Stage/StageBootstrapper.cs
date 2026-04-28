using UnityEngine;
using Sirenix.OdinInspector;

[DefaultExecutionOrder(int.MinValue)]
public class StageBootstrapper : MonoBehaviour
{
    [SerializeField]
    private CameraShakeAction _hitCameraShake;

#pragma warning disable
    private HitFeedbackHandler _hitFeedbackHandler;
    private EnemyKillRewardHandler _enemyKillRewardHandler;
    private HurdlePassRewardHandler _hurdlePassRewardHandler;
#pragma warning restore

    private void Start()
    {
        StageManager.Instance.Initialize();

        _hitFeedbackHandler = new(_hitCameraShake);
        _enemyKillRewardHandler = new();
        _hurdlePassRewardHandler = new();
    }

    private void OnDestroy()
    {
        _hitFeedbackHandler.Dispose();
        _enemyKillRewardHandler.Dispose();
        _hurdlePassRewardHandler.Dispose();
                
        if (PoolManager.HasInstance)
            PoolManager.Instance.Clear();
        if (StageManager.HasInstance)
            StageManager.Instance.Dispose();
    }
}