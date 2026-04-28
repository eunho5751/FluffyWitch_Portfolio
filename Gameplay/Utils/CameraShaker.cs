using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class CameraShaker : MonoBehaviour
{
    [SerializeField] 
    private float _duration = 0.3f;
    [SerializeField] 
    private float _magnitude = 0.2f;

    private Vector3 _originalPosition;
    private CancellationTokenSource _cts;

    private void Awake()
    {
        _originalPosition = transform.localPosition;
    }

    private void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }

    public void Shake()
    {
        Shake(_duration, _magnitude);
    }

    public void Shake(float duration, float magnitude)
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        ShakeAsync(duration, magnitude, _cts.Token).Forget();
    }

    private async UniTaskVoid ShakeAsync(float duration, float magnitude, CancellationToken token)
    {
        try
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                token.ThrowIfCancellationRequested();

                float progress = elapsed / duration;
                float dampening = 1f - Mathf.Pow(progress, 2);

                float offsetX = Random.Range(-1f, 1f) * magnitude * dampening;
                float offsetY = Random.Range(-1f, 1f) * magnitude * dampening;

                transform.localPosition = _originalPosition + new Vector3(offsetX, offsetY, 0f);

                elapsed += Time.unscaledDeltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update, token, true);
            }
        }
        finally
        {
            transform.localPosition = _originalPosition;
        }
    }
}