Shader "Bulletprooves/Unlit/UIPortalLensBlackHoleShader"
{
    Properties
    {
        [PerRendererData] _MainTex ("Render Texture", 2D) = "white" {}

        _Color ("Tint", Color) = (1, 1, 1, 1)
        _Brightness ("Brightness", Range(0, 3)) = 1

        _DistortionStrength ("Distortion Strength", Range(-1, 1)) = 0.06

        _BlackHoleStrength ("Black Hole Edge Strength", Range(0, 1)) = 0.3
        _BlackHoleRingRadius ("Black Hole Ring Radius", Range(0, 1)) = 0.82
        _BlackHoleRingWidth ("Black Hole Ring Width", Range(0.01, 0.5)) = 0.16
        _BlackHoleDarkness ("Black Hole Ring Darkness", Range(0, 1)) = 0.25

        _EdgeFeather ("Edge Feather", Range(0.001, 0.5)) = 0.08
        _EdgeDarkness ("Edge Darkness", Range(0, 1)) = 0.12

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)]
        _UseUIAlphaClip ("Use Alpha Clip", Float) = 0

        _LensCenter ("Lens Center", Vector) = (0.5, 0.5, 0, 0)
        _LensRadius ("Lens Radius", Float) = 0.15
        _ScreenAspect ("Screen Aspect", Float) = 1.777777
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
            Name "PortalLensWebGLLight"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color  : COLOR;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 position      : SV_POSITION;
                float4 color         : COLOR;
                float2 uv            : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _TextureSampleAdd;

            float4 _Color;
            float4 _ClipRect;

            float _Brightness;
            float _DistortionStrength;

            float _BlackHoleStrength;
            float _BlackHoleRingRadius;
            float _BlackHoleRingWidth;
            float _BlackHoleDarkness;

            float _EdgeFeather;
            float _EdgeDarkness;

            float4 _LensCenter;
            float _LensRadius;
            float _ScreenAspect;

            v2f vert(appdata input)
            {
                v2f output;
                output.worldPosition = input.vertex;
                output.position = UnityObjectToClipPos(input.vertex);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color * _Color;
                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                float2 uv = input.uv;

                float2 lensOffset = uv - _LensCenter.xy;
                lensOffset.x *= _ScreenAspect;

                float radius = max(_LensRadius, 0.0001);
                float distSq = dot(lensOffset, lensOffset);
                float invLen = rsqrt(max(distSq, 0.000001));
                float dist = distSq * invLen;
                float distanceFromCenter = dist / radius;
                float distance01 = saturate(distanceFromCenter);

                float2 direction = lensOffset * invLen;
                direction.x /= _ScreenAspect;

                float baseDistortion =
                    distance01 * distance01 * _DistortionStrength * radius;

                float ringMask =
                    saturate(1.0 - abs(distanceFromCenter - _BlackHoleRingRadius) / max(_BlackHoleRingWidth, 0.0001));

                ringMask = ringMask * ringMask * (3.0 - 2.0 * ringMask);
                ringMask *= distance01;
                ringMask *= saturate(1.0 - distanceFromCenter);

                float blackHolePull =
                    ringMask * _BlackHoleStrength * radius;

                float2 distortedUv =
                    uv - direction * (baseDistortion + blackHolePull);

                distortedUv = saturate(distortedUv);

                fixed4 color =
                    tex2D(_MainTex, distortedUv) + _TextureSampleAdd;

                color *= input.color;
                color.rgb *= _Brightness;

                float edgeAlpha =
                    1.0 - smoothstep(
                        1.0 - _EdgeFeather,
                        1.0,
                        distanceFromCenter
                    );

                float edgeDarkening =
                    lerp(
                        1.0,
                        1.0 - _EdgeDarkness,
                        distance01
                    );

                float blackHoleDarkening =
                    lerp(
                        1.0,
                        1.0 - _BlackHoleDarkness,
                        ringMask
                    );

                color.rgb *= edgeDarkening;
                color.rgb *= blackHoleDarkening;
                color.a *= edgeAlpha;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(input.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }
}