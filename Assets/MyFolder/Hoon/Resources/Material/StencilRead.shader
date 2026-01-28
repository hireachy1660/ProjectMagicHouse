Shader "Custom/StencilRead" {
    Properties { _MainTex ("Texture", 2D) = "white" {} }
    SubShader {
        Tags { "RenderType"="Opaque" }
        Pass {
            Stencil {
                Ref 1
                Comp Equal // 스텐실 값이 1인 곳(액자 안)에만 그림
            }
            // 아래는 일반적인 색칠 코드입니다.
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float2 uv : TEXCOORD0; float4 vertex : SV_POSITION; };
            sampler2D _MainTex;
            v2f vert (appdata v) { v2f o; o.vertex = UnityObjectToClipPos(v.vertex); o.uv = v.uv; return o; }
            fixed4 frag (v2f i) : SV_Target { return tex2D(_MainTex, i.uv); }
            ENDCG
        }
    }
}