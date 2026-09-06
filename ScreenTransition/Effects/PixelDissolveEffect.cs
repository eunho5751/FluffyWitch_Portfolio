using System;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;

public enum PixelDissolveGradientMode
{
    Directional = 0,
    Radial = 1,
}

[Serializable]
public class PixelDissolveEffectSettings
{
    [MinValue(0f)]
    public float Duration = 0.6f;

    [Title("Grid")]
    [Tooltip("레퍼런스 해상도 기준 블록 한 변의 크기(px). 블록 내부는 다시 4x4 디더로 나뉜다.")]
    [MinValue(2f)]
    public float BlockSize = 24f;

    [Title("Wipe")]
    public PixelDissolveGradientMode Mode = PixelDissolveGradientMode.Directional;

    [ShowIf(nameof(Mode), PixelDissolveGradientMode.Directional)]
    public Vector2 Direction = new(1f, -0.35f);

    [ShowIf(nameof(Mode), PixelDissolveGradientMode.Radial)]
    public Vector2 Center = new(0.5f, 0.5f);

    [Tooltip("그라디언트를 뒤집는다. Radial 모드에서 켜면 바깥에서 안쪽으로 조여든다.")]
    public bool InvertGradient;

    [Tooltip("0이면 그라디언트 순서 그대로, 1이면 완전 무작위로 블록이 채워진다.")]
    [Range(0f, 1f)]
    public float Randomness = 0.35f;

    [Tooltip("선두 전이 밴드의 두께. 클수록 디더가 넓게 흩어진다.")]
    [Range(0.01f, 1f)]
    public float EdgeWidth = 0.35f;

    [Title("Color")]
    public Color FillColor = Color.black;

    [Tooltip("채워지는 중인 블록에만 입혀지는 강조색.")]
    public Color EdgeColor = new(0.45f, 0.85f, 1f, 1f);

    [Range(0f, 1f)]
    public float EdgeIntensity = 1f;

    [Tooltip("난수 패턴 시드. 전환마다 다른 모양을 원하면 값을 바꾼다.")]
    public float Seed;
}

public class PixelDissolveEffect : ScreenTransitionEffectBase<PixelDissolveEffectSettings>
{
    private static readonly int _fillColorId = Shader.PropertyToID("_FillColor");
    private static readonly int _edgeColorId = Shader.PropertyToID("_EdgeColor");
    private static readonly int _edgeIntensityId = Shader.PropertyToID("_EdgeIntensity");
    private static readonly int _progressId = Shader.PropertyToID("_Progress");
    private static readonly int _gridSizeId = Shader.PropertyToID("_GridSize");
    private static readonly int _edgeWidthId = Shader.PropertyToID("_EdgeWidth");
    private static readonly int _randomnessId = Shader.PropertyToID("_Randomness");
    private static readonly int _directionId = Shader.PropertyToID("_Direction");
    private static readonly int _centerId = Shader.PropertyToID("_Center");
    private static readonly int _aspectId = Shader.PropertyToID("_Aspect");
    private static readonly int _modeId = Shader.PropertyToID("_Mode");
    private static readonly int _invertGradientId = Shader.PropertyToID("_InvertGradient");
    private static readonly int _reverseId = Shader.PropertyToID("_Reverse");
    private static readonly int _seedId = Shader.PropertyToID("_Seed");

    private static readonly Vector2 _fallbackResolution = new(1920f, 1080f);

    [SerializeField, Required]
    private Graphic _graphic;

    private Material _material;

    private void Awake()
    {
        // Instantiate 시 부모 캔버스를 정확히 덮도록 RectTransform을 강제로 스트레치한다
        var rectTransform = (RectTransform)transform;
        rectTransform.localScale = Vector3.one;
        rectTransform.localRotation = Quaternion.identity;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        // 공유 머티리얼 에셋이 런타임에 더럽혀지지 않도록 인스턴스를 따로 만든다
        _material = new Material(_graphic.material) { name = _graphic.material.name + " (Instance)" };
        _graphic.material = _material;
    }

    private void OnDestroy()
    {
        if (_material != null)
            Destroy(_material);
    }

    public override async UniTask StartTransitionInAsync() => await PlayAsync(true);
    public override async UniTask StartTransitionOutAsync() => await PlayAsync(false);

    private async UniTask PlayAsync(bool reverse)
    {
        ApplySettings(reverse);

        float duration = Mathf.Max(Settings.Duration, Mathf.Epsilon);
        float time = 0f;
        while (time < duration)
        {
            _material.SetFloat(_progressId, time / duration);

            await UniTask.NextFrame(destroyCancellationToken);
            time += ClampedDeltaTime;
        }

        _material.SetFloat(_progressId, 1f);
    }

    private void ApplySettings(bool reverse)
    {
        Vector2 size = _graphic.rectTransform.rect.size;
        if (size.x < 1f || size.y < 1f)
            size = _fallbackResolution;

        float blockSize = Mathf.Max(Settings.BlockSize, 2f);
        var grid = new Vector4(
            Mathf.Max(1f, Mathf.Round(size.x / blockSize)),
            Mathf.Max(1f, Mathf.Round(size.y / blockSize)),
            0f, 0f);

        _material.SetFloat(_progressId, 0f);
        _material.SetVector(_gridSizeId, grid);
        _material.SetFloat(_aspectId, size.x / size.y);
        _material.SetFloat(_modeId, (float)Settings.Mode);
        _material.SetVector(_directionId, Settings.Direction);
        _material.SetVector(_centerId, Settings.Center);
        _material.SetFloat(_invertGradientId, Settings.InvertGradient ? 1f : 0f);
        _material.SetFloat(_randomnessId, Settings.Randomness);
        _material.SetFloat(_edgeWidthId, Settings.EdgeWidth);
        _material.SetColor(_fillColorId, Settings.FillColor);
        _material.SetColor(_edgeColorId, Settings.EdgeColor);
        _material.SetFloat(_edgeIntensityId, Settings.EdgeIntensity);
        _material.SetFloat(_seedId, Settings.Seed);
        _material.SetFloat(_reverseId, reverse ? 1f : 0f);
    }
}
