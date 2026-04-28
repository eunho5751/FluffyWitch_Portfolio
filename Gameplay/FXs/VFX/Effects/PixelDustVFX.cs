using UnityEngine;

public class PixelDustVFX : MonoBehaviour
{
    [Tooltip("사용할 머티리얼")]
    public Material pixelMaterial;
 
    [Header("색상")]
    [Tooltip("파티클에 사용할 색상들")]
    public Color[] colors = new Color[]
    {
        new Color(0.36f, 0.43f, 0.88f), // 파랑
        new Color(0.25f, 0.25f, 0.45f), // 남색
        new Color(0.39f, 0.61f, 1.00f), // 하늘
        new Color(0.80f, 0.86f, 0.99f), // 연파랑
        new Color(0.52f, 0.49f, 0.53f), // 회색
        new Color(1.00f, 1.00f, 1.00f), // 흰색
    };
 
    [Header("파티클 설정")]
    [Tooltip("총 파티클 수")]
    [Range(10, 500)]
    public int particleCount = 80;
 
    [Tooltip("파티클이 퍼지는 최소 속도")]
    public float minSpeed = 2f;
 
    [Tooltip("파티클이 퍼지는 최대 속도")]
    public float maxSpeed = 6f;
 
    [Tooltip("시간에 따른 감속 비율 (0 = 감속 없음, 1 = 빠르게 멈춤)")]
    [Range(0f, 1f)]
    public float drag = 0.3f;
 
    [Tooltip("파티클 수명 (초)")]
    public float lifetime = 1.2f;
 
    [Header("스폰 범위")]
    [Tooltip("파티클이 생성되는 영역의 가로 크기 (월드 단위)")]
    public float spawnWidth = 1f;
 
    [Tooltip("파티클이 생성되는 영역의 세로 크기 (월드 단위)")]
    public float spawnHeight = 1f;
 
    [Header("픽셀 크기")]
    [Tooltip("파티클 하나의 월드 크기")]
    public float pixelWorldSize = 0.0625f;
 
    [Tooltip("작은 파티클이 섞일 확률 (0~1)")]
    [Range(0f, 1f)]
    public float smallParticleRatio = 0.3f;

    private ParticleSystem _ps;
 
    private void Awake()
    {
        _ps = CreateParticleSystem();
    }

    private void OnEnable()
    {
        EmitParticles();
    }

    private void EmitParticles()
    {
        Vector3 center = transform.position;
 
        for (int i = 0; i < particleCount; i++)
        {
            // 스폰 위치: 중심 주변 랜덤
            float ox = Random.Range(-spawnWidth * 0.5f, spawnWidth * 0.5f);
            float oy = Random.Range(-spawnHeight * 0.5f, spawnHeight * 0.5f);
 
            // 중심에서 바깥으로 향하는 방향 + 랜덤
            float angle = Mathf.Atan2(oy, ox) + Random.Range(-0.6f, 0.6f);
            float speed = Random.Range(minSpeed, maxSpeed);
 
            // 크기
            float size = pixelWorldSize;
            if (Random.value < smallParticleRatio)
                size *= 0.5f;
 
            // 색상
            Color color = colors[Random.Range(0, colors.Length)];
 
            var emitParams = new ParticleSystem.EmitParams
            {
                position = center + new Vector3(ox, oy, 0f),
                velocity = new Vector3(
                    Mathf.Cos(angle) * speed,
                    Mathf.Sin(angle) * speed,
                    0f
                ),
                startSize = size,
                startLifetime = lifetime * Random.Range(0.4f, 1f),
                startColor = color
            };
 
            _ps.Emit(emitParams, 1);
        }
    }
 
    private ParticleSystem CreateParticleSystem()
    {
        var ps = gameObject.AddComponent<ParticleSystem>();
 
        // 메인
        var main = ps.main;
        main.playOnAwake = false;
        main.loop = false;
        main.startLifetime = lifetime;
        main.startSpeed = 0f;
        main.startSize = pixelWorldSize;
        main.startColor = Color.white;
        main.gravityModifier = 0f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 5000;
 
        // Emission / Shape 끄기
        var emission = ps.emission;
        emission.enabled = false;
        var shape = ps.shape;
        shape.enabled = false;
 
        // 속도 감속 (Velocity Over Lifetime의 Speed Modifier로 점점 느려짐)
        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.speedModifier = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 1f - drag));
 
        // 크기: 서서히 줄어듦
        var sizeOverLife = ps.sizeOverLifetime;
        sizeOverLife.enabled = true;
        sizeOverLife.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 1f, 1f, 0.3f));
 
        // 페이드 아웃: 처음부터 서서히 투명해짐
        var col = ps.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(Color.white, 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.6f, 0.3f),
                new GradientAlphaKey(0.2f, 0.7f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        col.color = new ParticleSystem.MinMaxGradient(grad);
 
        // 렌더러
        var renderer = gameObject.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sharedMaterial = pixelMaterial;

        return ps;
    }
}