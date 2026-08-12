Shader "Bulletprooves/Unlit/SimpleDots4Web"
{
    Properties
    {
        _Color("Color", Color) = (0, 1, 1, 1)
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
        }

        LOD 100

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0  // WebGL2 / GLES3 수준 환경을 대상으로 컴파일

            #include "UnityCG.cginc"

            fixed4 _Color;

            struct appdata
            {
                float4 vertex : POSITION; // 메시의 로컬 정점 좌표
            };

            struct v2f
            {
                float4 vertex : SV_POSITION; // 화면에 그릴 최종 위치
            };

            v2f vert(appdata v)
            {
                v2f o;

                // 로컬 좌표를 클립 공간 좌표로 변환
                // 내부적으로 ObjectToWorld, View, Projection 행렬 계산이 들어감
                o.vertex = UnityObjectToClipPos(v.vertex);

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 조명 계산 없이 지정한 색상 그대로 출력
                return _Color;
            }

            ENDCG
        }
    }

    // 이 셰이더가 현재 환경에서 지원되지 않을 경우,
    // Unity 내장 단색 Unlit 셰이더를 대체로 사용
    FallBack "Unlit/Color"
}