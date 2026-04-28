using UnityEngine;

/// </summary>
public class CircleSpreadVFX : MonoBehaviour
{
    [Tooltip("사용할 머티리얼")]
    public Material circleMaterial;

    [Header("파티클 설정")]
    [Tooltip("한 번에 발사되는 파티클 수 (방향 수)")]
    [SerializeField] private int particlesPerBurst = 8;

    [Tooltip("퍼져나가는 속도")]
    [SerializeField] private float speed = 6f;

    [Tooltip("각 파티클의 수명")]
    [SerializeField] private float particleLifetime = 0.7f;

    [Tooltip("원 크기 (모두 동일)")]
    [SerializeField] private float circleSize = 0.4f;

    [Header("색상 설정")]
    [Tooltip("홀수 번째 발사 색상 (1, 3, 5...)")]
    [SerializeField] private Color oddBurstColor = Color.white;

    [Tooltip("짝수 번째 발사 색상 (2, 4, 6...)")]
    [SerializeField] private Color evenBurstColor = new Color(0.22f, 0.47f, 0.88f, 1f); // 파랑

    [Header("다중 발사 설정")]
    [Tooltip("총 발사 횟수")]
    [SerializeField] private int burstCount = 4;

    [Tooltip("발사 간격 (초)")]
    [SerializeField] private float burstInterval = 0.12f;

    [Header("기타")]
    [SerializeField] private bool useUnscaledTime = false;

    private ParticleSystem oddPS;
    private ParticleSystem evenPS;

    private void Awake()
    {
        oddPS = CreateChildParticleSystem("OddBurstPS", oddBurstColor, 0);
        evenPS = CreateChildParticleSystem("EvenBurstPS", evenBurstColor, 1);
    }

    private void OnEnable()
    {
        oddPS.Clear();
        evenPS.Clear();
        oddPS.Play();
        evenPS.Play();
    }

    /// <summary>
    /// 자식 ParticleSystem을 생성하고 해당 색상의 Burst만 등록합니다.
    /// startIndex: 0이면 홀수(0,2,4...), 1이면 짝수(1,3,5...)
    /// </summary>
    private ParticleSystem CreateChildParticleSystem(string name, Color color, int startIndex)
    {
        GameObject child = new GameObject(name);
        child.transform.SetParent(transform, false);
        child.transform.localPosition = Vector3.zero;

        ParticleSystem ps = child.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ParticleSystemRenderer psRenderer = child.GetComponent<ParticleSystemRenderer>();

        // 이 startIndex에 해당하는 버스트만 수집
        int myBurstCount = 0;
        for (int i = startIndex; i < burstCount; i += 2)
            myBurstCount++;

        float totalDuration = (burstCount - 1) * burstInterval + particleLifetime;
        
        // ── Main ──
        var main = ps.main;
        main.duration = totalDuration;
        main.loop = false;
        main.startLifetime = particleLifetime;
        main.startSpeed = speed;
        main.startSize = circleSize;
        main.startColor = color;
        main.maxParticles = particlesPerBurst * myBurstCount;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = false;
        main.gravityModifier = 0f;
        main.startRotation = 0f;
        main.useUnscaledTime = useUnscaledTime;

        // ── Emission ──
        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0;

        ParticleSystem.Burst[] bursts = new ParticleSystem.Burst[myBurstCount];
        int idx = 0;
        for (int i = startIndex; i < burstCount; i += 2)
        {
            bursts[idx] = new ParticleSystem.Burst(
                i * burstInterval,
                particlesPerBurst
            );
            idx++;
        }
        emission.SetBursts(bursts);

        // ── Shape ──
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.001f;
        shape.arcMode = ParticleSystemShapeMultiModeValue.BurstSpread;
        shape.arcSpread = 0f;
        shape.randomDirectionAmount = 0f;

        // ── Size over Lifetime (1 → 0 축소) ──
        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;

        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(new Keyframe(0f, 1f, 0f, -0.5f));
        sizeCurve.AddKey(new Keyframe(1f, 0f, -1.5f, 0f));
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        // ── 불필요한 모듈 비활성화 ──
        var col = ps.colorOverLifetime;       col.enabled = false;
        var vol = ps.velocityOverLifetime;    vol.enabled = false;
        var noise = ps.noise;                 noise.enabled = false;
        var collision = ps.collision;         collision.enabled = false;
        var sub = ps.subEmitters;             sub.enabled = false;
        var tsa = ps.textureSheetAnimation;   tsa.enabled = false;
        var trails = ps.trails;               trails.enabled = false;
        var lvol = ps.limitVelocityOverLifetime; lvol.enabled = false;
        var fol = ps.forceOverLifetime;       fol.enabled = false;
        var rol = ps.rotationOverLifetime;    rol.enabled = false;

        // ── Renderer ──
        psRenderer.sharedMaterial = circleMaterial;
        psRenderer.renderMode = ParticleSystemRenderMode.Billboard;

        return ps;
    }
}