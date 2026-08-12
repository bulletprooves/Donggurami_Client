Shader "Bulletprooves/Lit/BackUpCartoonShader"
{
    /////////////////////////////////////////////////////////////////////////////////////////////
    //                                                                                         //
    //                            Last Updated: 2026-06-26                                     //
    //                                                                                         //
    //            [밝음 / 중간(기준 1) / 어두움] 3개 단계의 명암 색상을 결정하는                  //
    //               (블렌더의 Color Ramp를 Constant 같은 느낌의) 카툰 쉐이더                    //
    //                                                                                         //
    /////////////////////////////////////////////////////////////////////////////////////////////
    
    Properties
    {
        _Color("Albedo", Color) = (1,1,1,1)
        _MainTex("Texture", 2D) = "white" {}

        // NOTE: 기준은 1
        _BrightFactor("Factor Bright", Range(1,2)) = 1.25
        _DarkFactor("Factor Dark", Range(0,1)) = 0.75
        _UpperThreshold("Threshold Upper", Range(0,1)) = 0.75 //보다 크면 bright
        _LowerThreshold("Threshold Lower", Range(0,1)) = 0.25 //보다 작으면 datk
    }

    SubShader
    {
        // 투명 없음
        Tags { "RenderType"="Opaque" }
        // NOTE: LOD 100 참고 https://docs.unity3d.com/kr/560/Manual/SL-ShaderLOD.html
        LOD 100
        
        Pass
        {
            Name "FORWARD"
            // Tags { "LightMode" = "ForwardBase" }
            // ㄴ> 이 코드는 그림자/감쇠/추가 라이트 않음.,
            // ㄴ> Lit ForwardBase Pass로 쓰면 사용, (빌트인 렌더 파이프라인에서 pragma 있어서 Main 라이트 제대로 받으려면 쓰는게 맞는데, 여기서 그림자/감쇠/추가 라이트 안씀)

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // 쉐이더 모델
            #pragma target 3.0
            // ㄴ> 5.0 은 WebGL에서 그려지지 않음 마젠타 나옴
            // #pragma multi_compile_fwdbase
            // ㄴ> 위에서 말했듯이  ForwardBase 안씀

            // _WorldSpaceLightPos0, _LightColor0 조명 관련 변수 써야해서 포함
            // UnityObjectToClipPos, UnityObjectToWorldNormal, TRANSFORM_TEX 도 씀
            #include "Lighting.cginc"

            // 메인 텍스쳐
            sampler2D _MainTex;

            // _MainTex의 Tiling / Offset 값, TRANSFORM_TEX에서 사용됨.
            float4 _MainTex_ST;

            // Properties
            fixed4 _Color;
            float _BrightFactor;
            float _DarkFactor;
            float _UpperThreshold;
            float _LowerThreshold;

            // Vertex Shader 로 들어오는 3d모델 원본 데이타 (법선 Normal 벡터가 필요해서 정의)
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                // 텍스쳐 좌표
                float2 uv : TEXCOORD0;
            };

            // Vertex Shader To Fragment Shader
            struct v2f
            {
                // 텍스쳐 샘플링
                float2 uv : TEXCOORD0;
                // 조명 방향도 월드 공간이라 법선벡터도 월드로 계산해야 함
                float3 normalDir : TEXCOORD1;
                // 최종 화면 벡터
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                // Object Space
                // ㄴ> 
                v2f o;

                // 1. 모델 버텍스르 화면에 그릴 좌표로 변환
                o.vertex = UnityObjectToClipPos(v.vertex);
                // 2. 법선 벡터
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                // 3. UV에 텍스처 Tiling / Offset
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                return o;
            }

            // Fragment Shader 픽셀 색상 결정
            float4 frag(v2f v) : SV_Target
            {
                // 텍스쳐에 색깔 곱해서 기본 색깔
                float4 texColor = tex2D(_MainTex, v.uv) * _Color;
                // 메인라이트 방향, _WorldSpaceLightPos0.xyz는 빌트인 팦라인에서 메인 라이트 정보를 담고 있음 (Directional Light 기준이다 이 말이야)
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                // Vertex Shader에서 넘긴 노멀은 픽셀 단위로 보간되면서 길이가 1 이 아닐 수 있기 때문에 normalize 다시 해주는 게 좋다고 함 (확인했을 때 차이를 잘 모르겠음)
                float3 normal = normalize(v.normalDir);

                // (법선, 빛방향) 벡터 내적 = dot(normal, lightDir) *이하 (Normal DOT LightDirection)ndotl로 표시하겠음*
                // 1: 정면으로 빛 받음
                // 0: 옆에서 빛 받는 거라 안받음
                // 음수: 반대 방향 (안쓰니까 max() 씀)
                float ndotl = max(0, dot(normal, lightDir));

                // if (ndotl >= _UpperThreshold)
                //     texColor.rgb *= _BrightFactor;
                // else if (ndotl <= _LowerThreshold)
                //     texColor.rgb *= _DarkFactor;
                // else
                //     texColor.rgb *= 1;//그대로

                // ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                // 위에 처럼 if 분기 타지말고, 아래처럼 step, lerp, smoothstep 참고해서 ㄱㄱ
                // vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv

                // step(x,y) : x <= y 이면 1, 아니면 0을 리턴 (ez remeber: 내려가면 0) http://www.silverwolf.co.kr/shader/79529
                float isBright = step(_UpperThreshold, ndotl);
                float isDark = step(ndotl, _LowerThreshold);
                float isMid = (1.0 - isBright) * (1.0 - isDark);
                // 밝기 범위별 색깔 계산
                float3 brightColor = texColor.rgb * _BrightFactor;
                float3 darkColor = texColor.rgb * _DarkFactor;
                float3 midColor = texColor.rgb;
                // 마스킹 (밝 + 닭 + 중) 이 중 한놈만 1 곱함
                float3 result = (brightColor * isBright) + (darkColor * isDark) + (midColor * isMid);

                // result *= _LightColor0.rgb;
                // ㄴ> Directional Light 색상 반영 필요시 사용 ㄱㄱ

                // 리턴: 카툰식 그림자 쉐이더, 원래 텍스쳐 알파값
                return float4(result, texColor.a);
            }
            ENDCG
        }
    }
}