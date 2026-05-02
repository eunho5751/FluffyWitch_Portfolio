using UnityEngine;
using Sirenix.OdinInspector;
using Cysharp.Threading.Tasks;

public class PoolAutoDespawner : MonoBehaviour
{
    [SerializeField, MinValue(0f)]
    private float _lifetime = 2f;

    private bool _isInitialized = false;

    private void OnEnable()
    {
        if (!_isInitialized)
            return;
        AutoDespawn().Forget();
    }

    private void Start()
    {
        _isInitialized = true;
        AutoDespawn().Forget();
    }

    private async UniTaskVoid AutoDespawn()
    {
        await UniTask.WaitForSeconds(_lifetime, cancellationToken: destroyCancellationToken);
        PoolManager.Instance.Release(gameObject);
    }
}