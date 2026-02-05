Shader "Custom/URP_StencilObject"
{
    Properties
    {
        // 텍스처를 넣을 수 있는 칸
        [MainTexture] _BaseMap("Base Texture", 2D) = "white" {}
        [MainColor] _BaseColor("Base Color", Color) = (1,1,1,1)
        [IntRange] _StencilID ("Stencil ID", Range(0, 255)) = 1
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }
        Pass
        {
            Stencil
            {
                Ref [_StencilID]
                Comp Equal
                Pass Keep
            }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { 
                float4 positionOS : POSITION; 
                float2 uv : TEXCOORD0; // 2. 텍스처 좌표(UV)를 받아옴
            };

            struct Varyings { 
                float4 positionCS : SV_POSITION; 
                float2 uv : TEXCOORD0; // 3. 픽셀에 전달할 UV
            };

            // 4. 변수들을 선언
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            float4 _BaseColor;

            Varyings vert(Attributes IN) {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv; // 5. 좌표를 넘겨줌
                return OUT;
            }

            // 6. 실제 색을 칠하는 fragment 함수
            half4 frag(Varyings IN) : SV_Target { 
                float4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                return texColor * _BaseColor; 
            }
            ENDHLSL
        }
    }
}