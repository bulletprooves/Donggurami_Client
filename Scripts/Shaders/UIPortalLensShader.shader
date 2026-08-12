Shader "Bulletprooves/Unlit/UIPortalLensShader"
{
    Properties
    {
        /*
         * _MainTex
         * UI RawImage나 Image가 실제로 표시할 텍스처.
         *
         * 여기서는 이름이 "Render Texture"라고 되어 있으므로,
         * 보통 다른 카메라가 찍은 RenderTexture를 넣어서
         * 포탈 너머 화면처럼 보여주는 용도라고 볼 수 있음.
         *
         * [PerRendererData]
         * Renderer마다 다른 값을 가질 수 있다는 의미.
         * Unity UI 쪽에서 각 Graphic/Image/RawImage가 자기 텍스처를 넣어줄 때 자주 사용함.
         */
        [PerRendererData] _MainTex ("Render Texture", 2D) = "white" {}

        /*
         * 색상 틴트.
         * 최종 색상에 곱해져서 전체적으로 색을 입힐 수 있음.
         *
         * 네가 적어둔 "Constant 사용 금지"는 Shader Graph 기준으로 맞는 말이야.
         * Graph 내부 Constant는 외부에서 MaterialPropertyBlock으로 바꾸는 값이 아니라
         * 셰이더 내부에 고정된 값처럼 취급되기 때문.
         *
         * 이렇게 Properties에 노출된 값은 Material이나 MaterialPropertyBlock으로 변경 가능.
         */
        _Color ("Tint", Color) = (1, 1, 1, 1)

        // 최종 RGB 밝기 배율.
        // 1이면 원본 밝기, 2면 두 배 밝게.
        _Brightness ("Brightness", Range(0, 3)) = 1

        /*
         * 렌즈 왜곡 강도.
         *
         * 양수:
         * 현재 픽셀이 자기 위치보다 중심 쪽 텍스처를 샘플링하게 됨.
         * 결과적으로 중앙이 확대된 것처럼 보일 수 있음.
         *
         * 음수:
         * 반대로 바깥쪽 텍스처를 샘플링함.
         * 오목렌즈 같은 느낌이 날 수 있음.
         */
        _DistortionStrength ("Distortion Strength", Range(-1, 1)) = 0.08

        /*
         * 왜곡이 중심에서 가장자리까지 어떻게 증가할지 결정.
         *
         * 값이 클수록:
         * 중심부는 거의 안 왜곡되고,
         * 가장자리 근처에서 급격히 왜곡됨.
         *
         * 값이 작을수록:
         * 중심부터 넓게 왜곡됨.
         */
        _DistortionFalloff ("Distortion Falloff", Range(0.1, 8)) = 2

        /*
         * 렌즈 가장자리 알파 페더링.
         *
         * 값이 작으면:
         * 가장자리가 날카롭게 잘림.
         *
         * 값이 크면:
         * 가장자리가 부드럽게 사라짐.
         */
        _EdgeFeather ("Edge Feather", Range(0.001, 0.5)) = 0.08

        /*
         * 렌즈 가장자리 어둡게 만들기.
         *
         * 0이면 어두워지지 않음.
         * 1이면 가장자리로 갈수록 많이 어두워짐.
         */
        _EdgeDarkness ("Edge Darkness", Range(0, 1)) = 0.15

        /*
         * 아래 Stencil 값들은 Unity UI Mask, RectMask2D와 관련 있음.
         * UI 셰이더에서 마스크가 정상 작동하려면 거의 필수로 들어가는 값들.
         */
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        /*
         * 어떤 색상 채널에 쓸지 결정.
         * 15는 RGBA 전체를 의미함.
         */
        _ColorMask ("Color Mask", Float) = 15

        /*
         * 알파가 거의 0인 픽셀을 버릴지 여부.
         * UI에서 알파 클리핑이 필요한 경우 사용.
         */
        [Toggle(UNITY_UI_ALPHACLIP)]
        _UseUIAlphaClip ("Use Alpha Clip", Float) = 0

        /*
         * 렌즈 중심.
         * UV 좌표 기준.
         *
         * UV는 보통:
         * 왼쪽 아래 = (0, 0)
         * 오른쪽 위 = (1, 1)
         *
         * 기본값 (0.5, 0.5)는 텍스처 정중앙.
         */
        _LensCenter ("Lens Center", Vector) = (0.5, 0.5, 0, 0)

        /*
         * 렌즈 반지름.
         * UV 기준 크기.
         *
         * 0.15면 텍스처 높이 기준 대략 15% 정도의 반지름.
         */
        _LensRadius ("Lens Radius", Float) = 0.15

        /*
         * 화면 또는 RawImage의 가로세로 비율.
         *
         * 예:
         * 16:9 화면이면 1.777777
         *
         * 이 값으로 x축 거리를 보정하지 않으면
         * 원형 렌즈가 가로/세로 비율에 따라 타원처럼 보일 수 있음.
         */
        _ScreenAspect ("Screen Aspect", Float) = 1.777777
    }

    SubShader
    {
        Tags
        {
            /*
             * Transparent:
             * 알파 블렌딩을 사용하는 투명 UI로 렌더링.
             */
            "Queue" = "Transparent"

            /*
             * Projector 영향 무시.
             * UI 셰이더에서는 보통 필요 없음.
             */
            "IgnoreProjector" = "True"

            "RenderType" = "Transparent"

            /*
             * 머티리얼 프리뷰에서 Plane 형태로 보이게 함.
             */
            "PreviewType" = "Plane"

            /*
             * Unity Sprite Atlas 사용 가능.
             * UI 기본 셰이더 스타일과 맞추기 위한 태그.
             */
            "CanUseSpriteAtlas" = "True"
        }

        /*
         * Unity UI Mask 지원용 Stencil 설정.
         *
         * Mask 컴포넌트는 Stencil Buffer를 이용해서
         * 특정 UI 영역 안에서만 자식 UI가 보이게 만들 수 있음.
         */
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        /*
         * UI는 양면 렌더링을 하는 경우가 많으므로 뒷면 제거 끔.
         */
        Cull Off

        /*
         * 빛 계산 안 함.
         * Unlit 셰이더이므로 조명 영향을 받지 않음.
         */
        Lighting Off

        /*
         * 깊이 버퍼에 쓰지 않음.
         * 투명 UI에서는 보통 ZWrite Off.
         */
        ZWrite Off

        /*
         * Unity UI의 ZTest 방식 사용.
         * Canvas 렌더링 방식에 맞춰 Unity가 값을 넣어줌.
         */
        ZTest [unity_GUIZTestMode]

        /*
         * 일반적인 알파 블렌딩.
         *
         * SrcAlpha OneMinusSrcAlpha:
         * 최종색 = 현재색 * 알파 + 배경색 * (1 - 알파)
         */
        Blend SrcAlpha OneMinusSrcAlpha

        /*
         * 쓸 색상 채널 지정.
         */
        ColorMask [_ColorMask]

        Pass
        {
            Name "PortalLens"

            CGPROGRAM

            /*
             * vert 함수는 Vertex Shader로 사용.
             * frag 함수는 Fragment Shader로 사용.
             */
            #pragma vertex vert
            #pragma fragment frag

            /*
             * Shader Model 2.0 타겟.
             * 비교적 낮은 사양까지 지원하려는 설정.
             */
            #pragma target 2.0

            /*
             * Unity 기본 셰이더 유틸 함수들.
             * UnityObjectToClipPos, TRANSFORM_TEX 같은 함수/매크로가 들어있음.
             */
            #include "UnityCG.cginc"

            /*
             * Unity UI용 함수.
             * UnityGet2DClipping 같은 UI 클리핑 함수가 들어있음.
             */
            #include "UnityUI.cginc"

            /*
             * UI Clip Rect 지원 여부에 따라 셰이더 변형을 만듦.
             * RectMask2D 같은 기능과 관련 있음.
             */
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT

            /*
             * UI Alpha Clip 사용 여부에 따라 셰이더 변형을 만듦.
             */
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            /*
             * Vertex Shader로 들어오는 원본 데이터.
             *
             * UI도 결국 사각형 Mesh로 렌더링됨.
             * RawImage라면 보통 사각형 Quad의 네 꼭짓점 데이터가 들어온다고 보면 됨.
             */
            struct appdata
            {
                // 오브젝트 로컬 좌표의 정점 위치
                float4 vertex : POSITION;

                // UI Graphic의 Vertex Color
                // Image color, CanvasRenderer color 등이 섞여 들어올 수 있음
                float4 color : COLOR;

                // 텍스처 좌표
                float2 uv : TEXCOORD0;
            };

            /*
             * Vertex Shader에서 Fragment Shader로 넘겨줄 데이터.
             */
            struct v2f
            {
                // 클립 공간 위치. GPU가 화면에 그릴 때 사용.
                float4 position : SV_POSITION;

                // 최종적으로 픽셀 색상에 곱할 색
                float4 color : COLOR;

                // 텍스처 샘플링용 UV
                float2 uv : TEXCOORD0;

                /*
                 * UI 클리핑 계산용 위치.
                 *
                 * 이름은 worldPosition이지만,
                 * Unity UI 기본 셰이더에서도 input.vertex를 넣어 사용함.
                 * RectMask2D 계산에 필요.
                 */
                float4 worldPosition : TEXCOORD1;
            };

            // 메인 텍스처
            sampler2D _MainTex;

            /*
             * _MainTex의 Tiling / Offset 값.
             * TRANSFORM_TEX(input.uv, _MainTex)를 쓰면 이 값이 반영됨.
             */
            float4 _MainTex_ST;

            /*
             * Unity UI 셰이더에서 종종 사용하는 보정값.
             * 특정 압축 텍스처/알파 처리에서 필요할 수 있음.
             */
            float4 _TextureSampleAdd;

            // Properties에서 선언한 값들이 실제 셰이더 변수로 들어옴
            float4 _Color;
            float4 _ClipRect;

            float _Brightness;
            float _DistortionStrength;
            float _DistortionFalloff;
            float _EdgeFeather;
            float _EdgeDarkness;

            float4 _LensCenter;
            float _LensRadius;
            float _ScreenAspect;

            /*
             * Vertex Shader
             *
             * 역할:
             * 1. UI 정점 위치를 화면에 그릴 수 있는 좌표로 변환
             * 2. UV 전달
             * 3. 색상 전달
             */
            v2f vert(appdata input)
            {
                v2f output;

                // UI 클리핑 계산용 위치 저장
                output.worldPosition = input.vertex;

                // 로컬 공간 정점 좌표를 클립 공간으로 변환
                output.position = UnityObjectToClipPos(input.vertex);

                // UV에 Tiling / Offset 적용
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);

                // 정점 색상과 머티리얼 Tint 색상을 곱함
                output.color = input.color * _Color;

                return output;
            }

            /*
             * Fragment Shader
             *
             * 역할:
             * 실제 픽셀 하나하나의 색상을 결정함.
             *
             * 이 셰이더의 핵심은 여기 있음.
             */
            fixed4 frag(v2f input) : SV_Target
            {
                /*
                 * 현재 픽셀의 UV가 렌즈 중심으로부터 얼마나 떨어져 있는지 계산.
                 *
                 * 예:
                 * input.uv = (0.6, 0.5)
                 * _LensCenter = (0.5, 0.5)
                 *
                 * lensOffset = (0.1, 0.0)
                 *
                 * 즉, 중심에서 오른쪽으로 0.1만큼 떨어져 있다는 뜻.
                 */
                float2 lensOffset = input.uv - _LensCenter.xy;

                /*
                 * 화면 비율 보정.
                 *
                 * UV 좌표는 x, y가 둘 다 0~1이지만,
                 * 실제 화면은 보통 가로가 더 김.
                 *
                 * 그래서 x축 거리를 더 크게 계산해줘야
                 * 시각적으로 원형 렌즈가 유지됨.
                 */
                lensOffset.x *= _ScreenAspect;

                /*
                 * 중심으로부터의 거리를 구하고,
                 * 렌즈 반지름 기준으로 정규화함.
                 *
                 * distanceFromCenter가:
                 * 0이면 렌즈 중심
                 * 0.5면 반지름의 절반 지점
                 * 1이면 렌즈 가장자리
                 * 1보다 크면 렌즈 바깥
                 *
                 * max(_LensRadius, 0.0001)는 0으로 나누기 방지용.
                 */
                float distanceFromCenter =
                    length(lensOffset) / max(_LensRadius, 0.0001);

                /*
                 * 왜곡 강도 계산.
                 *
                 * saturate(distanceFromCenter):
                 * 값을 0~1 사이로 제한.
                 *
                 * pow(..., _DistortionFalloff):
                 * 중심에서 가장자리까지 왜곡이 증가하는 곡선을 만듦.
                 *
                 * _DistortionStrength:
                 * 전체 왜곡 방향과 강도.
                 *
                 * _LensRadius:
                 * 렌즈 크기에 비례해서 왜곡량도 조절.
                 */
                float distortionAmount =
                    pow(saturate(distanceFromCenter), _DistortionFalloff)
                    * _DistortionStrength
                    * _LensRadius;

                /*
                 * 중심에서 현재 픽셀 방향으로 향하는 단위 벡터.
                 *
                 * normalize(0, 0)을 하면 문제가 생길 수 있으므로
                 * 아주 작은 값을 더해서 0 벡터를 피함.
                 */
                float2 direction = normalize(
                    lensOffset + float2(0.00001, 0.00001)
                );

                /*
                 * 위에서 lensOffset.x에 _ScreenAspect를 곱했으므로,
                 * 다시 UV 좌표계로 돌아오기 위해 x 방향을 나눠줌.
                 */
                direction.x /= _ScreenAspect;

                /*
                 * 왜곡된 UV 계산.
                 *
                 * 원래는 input.uv 위치의 색을 가져와야 하지만,
                 * 여기서는 direction 방향으로 조금 이동한 위치의 색을 가져옴.
                 *
                 * 즉, 픽셀 위치는 그대로인데
                 * "어디의 텍스처 색을 가져올지"를 바꾸는 방식.
                 *
                 * 이것이 렌즈 왜곡의 핵심.
                 */
                float2 distortedUv =
                    input.uv - direction * distortionAmount;

                /*
                 * 왜곡된 UV 위치에서 텍스처 색을 샘플링.
                 *
                 * tex2D(_MainTex, distortedUv):
                 * RenderTexture에서 해당 UV 위치의 색을 읽음.
                 */
                fixed4 color =
                    tex2D(_MainTex, distortedUv) + _TextureSampleAdd;

                /*
                 * UI 색상과 Tint 색상을 곱함.
                 */
                color *= input.color;

                /*
                 * 밝기 조절.
                 * 알파는 건드리지 않고 RGB만 조절.
                 */
                color.rgb *= _Brightness;

                /*
                 * 렌즈 가장자리 알파 페더링.
                 *
                 * smoothstep(a, b, x)는:
                 * x가 a보다 작으면 0
                 * x가 b보다 크면 1
                 * a~b 사이에서는 부드럽게 0에서 1로 증가
                 *
                 * 여기서는:
                 * distanceFromCenter가 1 - _EdgeFeather보다 작으면 거의 1
                 * distanceFromCenter가 1에 가까워지면 0으로 사라짐
                 *
                 * 즉, 렌즈 가장자리가 부드럽게 투명해짐.
                 */
                float edgeAlpha =
                    1.0 - smoothstep(
                        1.0 - _EdgeFeather,
                        1.0,
                        distanceFromCenter
                    );

                /*
                 * 렌즈 가장자리 어둡게 만들기.
                 *
                 * 중심에서는 1.0,
                 * 가장자리로 갈수록 1.0 - _EdgeDarkness에 가까워짐.
                 *
                 * 예:
                 * _EdgeDarkness = 0.15라면
                 * 가장자리에서는 RGB가 0.85배 정도로 어두워짐.
                 */
                float edgeDarkening =
                    lerp(
                        1.0,
                        1.0 - _EdgeDarkness,
                        saturate(distanceFromCenter)
                    );

                // 가장자리 어두움 적용
                color.rgb *= edgeDarkening;

                // 가장자리 투명도 적용
                color.a *= edgeAlpha;

                /*
                 * RectMask2D 같은 UI 클리핑 처리.
                 *
                 * UNITY_UI_CLIP_RECT가 켜져 있을 때만 컴파일됨.
                 */
                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(input.worldPosition.xy, _ClipRect);
                #endif

                /*
                 * 알파 클리핑.
                 *
                 * color.a가 0.001보다 작으면 픽셀을 버림.
                 * 완전 투명한 픽셀을 렌더링하지 않게 할 수 있음.
                 */
                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }
}