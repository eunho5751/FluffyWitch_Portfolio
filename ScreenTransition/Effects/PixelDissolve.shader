Shader "UI/ScreenTransition/PixelDissolve"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _FillColor ("Fill Color", Color) = (0,0,0,1)
        _EdgeColor ("Edge Color", Color) = (0.45,0.85,1,1)
        _EdgeIntensity ("Edge Intensity", Range(0,1)) = 1

        _Progress ("Progress", Range(0,1)) = 0
        _GridSize ("Grid Size (cells)", Vector) = (80,45,0,0)
        _EdgeWidth ("Edge Width", Range(0.01,1)) = 0.35
        _Randomness ("Randomness", Range(0,1)) = 0.35
        _Direction ("Direction", Vector) = (1,-0.35,0,0)
        _Center ("Radial Center", Vector) = (0.5,0.5,0,0)
        _Aspect ("Aspect", Float) = 1.7777
        _Mode ("Mode (0=Directional 1=Radial)", Float) = 0
        _InvertGradient ("Invert Gradient", Float) = 0
        _Reverse ("Reverse", Float) = 0
        _Seed ("Seed", Float) = 0

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "PIXEL_DISSOLVE"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex        : SV_POSITION;
                fixed4 color         : COLOR;
                float2 texcoord      : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float4 _ClipRect;

            float4 _FillColor;
            float4 _EdgeColor;
            float  _EdgeIntensity;
            float  _Progress;
            float4 _GridSize;
            float  _EdgeWidth;
            float  _Randomness;
            float4 _Direction;
            float4 _Center;
            float  _Aspect;
            float  _Mode;
            float  _InvertGradient;
            float  _Reverse;
            float  _Seed;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color * _Color;
                return OUT;
            }

            // 셀 좌표 -> 0..1 난수
            float Hash21(float2 p, float seed)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031 + seed * 0.7317);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            // 정규 순서 디더 행렬 (2x2 -> 4x4 재귀 구성)
            float Bayer2(float2 a)
            {
                a = floor(a);
                return frac(a.x * 0.5 + a.y * a.y * 0.75);
            }

            float Bayer4(float2 a)
            {
                return Bayer2(a * 0.5) * 0.25 + Bayer2(a);
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord;
                float2 grid = max(_GridSize.xy, float2(1.0, 1.0));

                float2 cell = floor(uv * grid);
                float2 cellUV = (cell + 0.5) / grid;

                // 셀 단위 진행 순서를 정하는 그라디언트
                float grad;
                if (_Mode < 0.5)
                {
                    float2 dir = _Direction.xy;
                    dir = (dot(dir, dir) < 1e-6) ? float2(1.0, 0.0) : normalize(dir);
                    float halfSpan = 0.5 * (abs(dir.x) + abs(dir.y));
                    grad = saturate(dot(cellUV - 0.5, dir) / (2.0 * halfSpan) + 0.5);
                }
                else
                {
                    float2 asp = float2(_Aspect, 1.0);
                    float maxD = 0.0;
                    maxD = max(maxD, length((float2(0.0, 0.0) - _Center.xy) * asp));
                    maxD = max(maxD, length((float2(1.0, 0.0) - _Center.xy) * asp));
                    maxD = max(maxD, length((float2(0.0, 1.0) - _Center.xy) * asp));
                    maxD = max(maxD, length((float2(1.0, 1.0) - _Center.xy) * asp));
                    grad = saturate(length((cellUV - _Center.xy) * asp) / max(maxD, 1e-5));
                }
                grad = lerp(grad, 1.0 - grad, saturate(_InvertGradient));

                float threshold = lerp(grad, Hash21(cell, _Seed), saturate(_Randomness));

                // 선두에 폭 _EdgeWidth 짜리 전이 밴드를 두고 셀을 서서히 채운다
                float edgeW = max(_EdgeWidth, 1e-4);
                float coverage = saturate((_Progress * (1.0 + edgeW) - threshold) / edgeW);

                // 셀 내부를 4x4 디더로 채워 픽셀아트 질감 유지
                float2 sub = floor(frac(uv * grid) * 4.0);
                float filled = step(Bayer4(sub) + 0.03125, coverage);

                float alpha = lerp(filled, 1.0 - filled, saturate(_Reverse));

                // 진행 중인 셀에만 강조색. 프론트에서 가장 진하고 뒤로 갈수록 잦아든다
                float band = step(0.0001, coverage) * step(coverage, 0.9999);
                float edgeMask = band * lerp(1.0 - coverage, coverage, saturate(_Reverse)) * saturate(_EdgeIntensity);
                float4 col = lerp(_FillColor, _EdgeColor, edgeMask);

                col *= IN.color;
                col.a *= alpha;

                #ifdef UNITY_UI_CLIP_RECT
                col.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                clip(col.a - 0.001);
                return col;
            }
        ENDCG
        }
    }
}
