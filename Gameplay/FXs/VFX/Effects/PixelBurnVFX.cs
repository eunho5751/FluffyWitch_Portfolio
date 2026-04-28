using UnityEngine;

/// <summary>
/// 캐릭터 부착용 심플 픽셀 불꽃 VFX (ParticleSystem 기반)
/// 
/// [사용법]
/// 1. 캐릭터 자식 오브젝트에 이 스크립트 부착
/// 2. Play → 자동으로 ParticleSystem + 텍스처 + 머티리얼 생성
/// 
/// 별도 에셋 필요 없음. 스크립트 하나로 완결.
/// </summary>
public class PixelBurnVFX : MonoBehaviour
{
    [Header("=== 파티클 ===")]
    [Tooltip("동시 최대 파티클 수")]
    [Range(10, 60)]
    public int maxParticles = 30;

    [Tooltip("초당 방출 수")]
    [Range(5, 40)]
    public float emissionRate = 18f;

    [Header("=== 크기 & 모양 ===")]
    [Tooltip("파티클 최소 크기")]
    [Range(0.01f, 0.2f)]
    public float minSize = 0.04f;

    [Tooltip("파티클 최대 크기")]
    [Range(0.02f, 0.4f)]
    public float maxSize = 0.12f;

    [Tooltip("방출 너비")]
    [Range(0.05f, 1f)]
    public float spawnWidth = 0.15f;

    [Header("=== 움직임 ===")]
    [Tooltip("상승 속도")]
    [Range(0.2f, 3f)]
    public float riseSpeed = 0.8f;

    [Tooltip("수명")]
    [Range(0.2f, 2f)]
    public float lifetime = 0.7f;

    [Tooltip("좌우 흔들림")]
    [Range(0f, 1f)]
    public float drift = 0.2f;

    [Header("=== 색상 ===")]
    public Color32 coreYellow = new Color32(255, 220, 50, 255);
    public Color32 midOrange = new Color32(230, 140, 20, 255);
    public Color32 darkBrown = new Color32(120, 60, 10, 230);
    public Color32 dimEmber  = new Color32(50, 20, 3, 140);

    private ParticleSystem ps;

    void Start()
    {
        CreateParticleSystem();
    }

    void CreateParticleSystem()
    {
        ps = gameObject.AddComponent<ParticleSystem>();

        // --- 렌더러 ---
        var renderer = GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = CreateMaterial();

        // --- 메인 ---
        var main = ps.main;
        main.loop = true;
        main.playOnAwake = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(lifetime * 0.6f, lifetime);
        main.startSpeed = new ParticleSystem.MinMaxCurve(riseSpeed * 0.5f, riseSpeed);
        main.startSize = new ParticleSystem.MinMaxCurve(minSize, maxSize);
        main.maxParticles = maxParticles;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.gravityModifier = -0.1f;
        main.startRotation = 0f;

        // --- 방출 ---
        var emission = ps.emission;
        emission.rateOverTime = emissionRate;

        // --- 모양: 얇은 직선에서 위로 ---
        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Rectangle;
        shape.scale = new Vector3(spawnWidth, 0.01f, 0.01f);
        shape.rotation = new Vector3(0f, 0f, 0f);

        // --- 수명별 색상 ---
        var col = ps.colorOverLifetime;
        col.enabled = true;

        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey((Color)coreYellow, 0f),
                new GradientColorKey((Color)coreYellow, 0.15f),
                new GradientColorKey((Color)midOrange, 0.35f),
                new GradientColorKey((Color)darkBrown, 0.65f),
                new GradientColorKey((Color)dimEmber, 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(0.137f, 0f),
                new GradientAlphaKey(0.196f, 0.1f),
                new GradientAlphaKey(0.784f, 0.4f),
                new GradientAlphaKey(0.5f, 0.75f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        col.color = grad;

        // --- 수명별 크기 ---
        var sol = ps.sizeOverLifetime;
        sol.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(new Keyframe(0f, 1f, 0f, 0f));
        sizeCurve.AddKey(new Keyframe(0.3f, 1f, 0f, 0f));
        sizeCurve.AddKey(new Keyframe(0.7f, 0.7f, 0f, 0f));
        sizeCurve.AddKey(new Keyframe(1f, 0.3f, 0f, 0f));
        sol.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        // --- 노이즈 (가벼운 흔들림) ---
        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = drift;
        noise.frequency = 2f;
        noise.scrollSpeed = 1f;
        noise.octaveCount = 1;
        noise.damping = true;

        // --- 속도 제한 ---
        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.x = new ParticleSystem.MinMaxCurve(-drift * 0.5f, drift * 0.5f);
        vel.y = new ParticleSystem.MinMaxCurve(riseSpeed * 0.3f, riseSpeed);
        vel.z = new ParticleSystem.MinMaxCurve(0f, 0f);

        ps.Play();
    }

    Material CreateMaterial()
    {
        // 사각형 픽셀 텍스처 생성 (글로우 없는 깔끔한 사각형)
        int size = 4;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;

        // 꽉 찬 흰색 사각형 (색상은 파티클 컬러로 제어)
        Color32 white = new Color32(255, 255, 255, 255);
        Color32[] pixels = new Color32[size * size];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = white;
        tex.SetPixels32(pixels);
        tex.Apply();

        // Additive가 아닌 일반 알파 블렌딩 → 은은한 느낌
        Material mat = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));
        mat.mainTexture = tex;
        return mat;
    }

    // === Public API ===

    public void SetActive(bool active)
    {
        if (ps == null) return;
        if (active) ps.Play();
        else ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    public void SetIntensity(float t)
    {
        if (ps == null) return;
        t = Mathf.Clamp01(t);
        var emission = ps.emission;
        emission.rateOverTime = emissionRate * t;
    }

    void OnDestroy()
    {
        if (ps != null)
        {
            var r = GetComponent<ParticleSystemRenderer>();
            if (r != null && r.material != null)
            {
                if (r.material.mainTexture != null)
                    Destroy(r.material.mainTexture);
                Destroy(r.material);
            }
        }
    }
}