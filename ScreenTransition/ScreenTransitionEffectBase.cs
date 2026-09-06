using UnityEngine;
using Cysharp.Threading.Tasks;

public abstract class ScreenTransitionEffectBase : MonoBehaviour
{
    // 씬 로드 직후의 unscaledDeltaTime 스파이크가 전환을 즉시 완료시키지 않도록 클램프한다
    private const float MaxDeltaTime = 1f / 30f;

    public abstract UniTask StartTransitionInAsync();
    public abstract UniTask StartTransitionOutAsync();
    internal abstract void SetSettings(object settings);

    protected static float ClampedDeltaTime => Mathf.Min(Time.unscaledDeltaTime, MaxDeltaTime);
}

public abstract class ScreenTransitionEffectBase<T> : ScreenTransitionEffectBase
{
    internal sealed override void SetSettings(object settings) => Settings = (T)settings;
    public T Settings { get; private set; }
}