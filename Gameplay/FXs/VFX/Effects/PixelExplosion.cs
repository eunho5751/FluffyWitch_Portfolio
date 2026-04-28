using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 픽셀 아트 스타일 2D 폭발 이펙트 (Flash + Fire 레이어만).
///
/// 진짜 픽셀 퍼펙트 렌더링을 위해서는:
///   1) Window > Package Manager > 2D Pixel Perfect 설치
///   2) 메인 카메라에 Pixel Perfect Camera 컴포넌트 추가
///   3) Assets PPU = 16 (이 스크립트의 pixelsPerUnit과 동일하게)
///   4) Pixel Snapping 옵션 체크
/// </summary>
[DisallowMultipleComponent]
public class PixelExplosion : MonoBehaviour
{
    [Header("텍스처 (모두 흰색 권장 - 색상은 코드에서 입힙니다)")]
    public Texture2D midCircleTex;
    public Texture2D sparkleTex;

    [Header("기본 설정")]
    [Tooltip("픽셀 1개당 월드 단위 크기. 카메라의 Assets PPU와 일치시킬 것")]
    public float pixelsPerUnit = 16f;

    [Header("재생")]
    public bool playOnAwake = true;

    [Header("색상 팔레트")]
    public Color flashColor = new Color(1f, 1f, 0.85f, 1f);
    public Color fireHot    = new Color(1f, 0.95f, 0.4f, 1f);
    public Color fireMid    = new Color(1f, 0.45f, 0.1f, 1f);
    public Color fireCool   = new Color(0.7f, 0.1f, 0.05f, 1f);

    private readonly List<ParticleSystem> systems = new List<ParticleSystem>();

    // 1픽셀 크기 (월드 단위)
    private float Px => 1f / pixelsPerUnit;
    // 사이즈를 픽셀 단위로 정렬
    private float PixSize(float pixels) => Mathf.Max(1, Mathf.Round(pixels)) * Px;

    void Awake()
    {
        BuildEffect();
        if (playOnAwake) Play();
    }

    public void Play()
    {
        foreach (var ps in systems) if (ps != null) ps.Play();
    }

    public void Stop()
    {
        foreach (var ps in systems) if (ps != null) ps.Stop();
    }

    void BuildEffect()
    {
        systems.Add(BuildFire  ("Fire",  midCircleTex, 0));
        systems.Add(BuildFlash ("Flash", sparkleTex,   1));
    }

    // ---------- 1) Flash (섬광) ----------
    ParticleSystem BuildFlash(string name, Texture2D tex, int order)
    {
        var ps = CreateChild(name, tex, order);
        var main = ps.main;
        main.duration = 0.15f;
        main.loop = false;
        main.startLifetime = 0.18f;
        main.startSpeed = 0f;
        main.startSize = PixSize(6);
        main.startColor = flashColor;
        main.startRotation = 0f;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.maxParticles = 4;

        var em = ps.emission;
        em.rateOverTime = 0;
        em.SetBursts(new[] { new ParticleSystem.Burst(0f, 1) });

        var shape = ps.shape;
        shape.enabled = false;

        var sizeOL = ps.sizeOverLifetime;
        sizeOL.enabled = true;
        sizeOL.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 0.4f),
            new Keyframe(0.3f, 1.5f),
            new Keyframe(1f, 0.2f)
        ));

        var colOL = ps.colorOverLifetime;
        colOL.enabled = true;
        colOL.color = new ParticleSystem.MinMaxGradient(MakeFadeGradient(flashColor, 1f, 0.5f, 0f));

        return ps;
    }

    // ---------- 2) Fire (화염) ----------
    ParticleSystem BuildFire(string name, Texture2D tex, int order)
    {
        var ps = CreateChild(name, tex, order);
        var main = ps.main;
        main.duration = 0.6f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.45f, 0.8f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 4f);
        main.startSize = new ParticleSystem.MinMaxCurve(PixSize(5), PixSize(8));
        main.startRotation = 0f;
        main.gravityModifier = -0.3f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 60;

        var em = ps.emission;
        em.rateOverTime = 0;
        em.SetBursts(new[] { new ParticleSystem.Burst(0f, 28) });

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.3f;
        shape.radiusThickness = 1f;

        var grad = new Gradient();
        grad.SetKeys(
            new[] {
                new GradientColorKey(fireHot,  0f),
                new GradientColorKey(fireMid,  0.4f),
                new GradientColorKey(fireCool, 0.8f),
                new GradientColorKey(new Color(0.1f, 0.05f, 0.05f), 1f)
            },
            new[] {
                new GradientAlphaKey(0.5f, 0f),
                new GradientAlphaKey(0.75f, 0.7f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        var colOL = ps.colorOverLifetime;
        colOL.enabled = true;
        colOL.color = new ParticleSystem.MinMaxGradient(grad);

        var sizeOL = ps.sizeOverLifetime;
        sizeOL.enabled = true;
        // 끝까지 크기를 거의 유지 (1.0 → 최대 1.1 → 0.75)
        sizeOL.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(0.3f, 1.1f),
            new Keyframe(1f, 0.5f)
        ));

        // 강한 감쇠로 초기 속도만 잠깐 쓰고 거의 제자리에서 타오르게
        var limit = ps.limitVelocityOverLifetime;
        limit.enabled = true;
        limit.dampen = 0.7f;
        limit.limit = 0.3f;

        return ps;
    }

    // ---------------------------------------------------------------------
    // 헬퍼
    // ---------------------------------------------------------------------
    ParticleSystem CreateChild(string name, Texture2D tex, int order)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);

        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        // transform의 lossyScale이 입자 크기/속도/이미터 형태에 반영되도록
        var main = ps.main;
        main.scalingMode = ParticleSystemScalingMode.Hierarchy;

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.alignment = ParticleSystemRenderSpace.View;
        renderer.sortingOrder = order;
        renderer.minParticleSize = 0f;
        renderer.maxParticleSize = 1f;

        if (tex != null)
        {
            tex.filterMode = FilterMode.Point;
        }

        // URP 2D 셰이더 사용 (Built-in 사용자는 "Sprites/Default"로 변경)
        Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        var mat = new Material(shader);
        if (tex != null) mat.mainTexture = tex;
        renderer.material = mat;

        return ps;
    }

    Gradient MakeFadeGradient(Color c, float a0, float a1, float a2)
    {
        var g = new Gradient();
        g.SetKeys(
            new[] {
                new GradientColorKey(c, 0f),
                new GradientColorKey(c, 1f)
            },
            new[] {
                new GradientAlphaKey(a0, 0f),
                new GradientAlphaKey(a1, 0.5f),
                new GradientAlphaKey(a2, 1f)
            }
        );
        return g;
    }
}