using System;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;

[Serializable]
public class SimpleFadeEffectSettings
{
    [MinValue(0f)]
    public float Duration;
}

public class SimpleFadeEffect : ScreenTransitionEffectBase<SimpleFadeEffectSettings>
{
    [SerializeField]
    private Graphic _graphic;

    public async override UniTask StartTransitionInAsync() => await FadeAsync(1f, 0f);
    public async override UniTask StartTransitionOutAsync() => await FadeAsync(0f, 1f);

    private async UniTask FadeAsync(float from, float to)
    {
        Color c = Color.black;
        float time = 0f;
        while (time < Settings.Duration)
        {
            c.a = Mathf.Lerp(from, to, time / Settings.Duration);
            _graphic.color = c;

            await UniTask.NextFrame(destroyCancellationToken);
            time += ClampedDeltaTime;
        }

        c.a = to;
        _graphic.color = c;
    }
}
