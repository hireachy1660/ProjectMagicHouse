Shader "Custom/URP_StencilObject"
{
    Properties
    {
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
            #pragma multi_compile_instancing
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes { 
                float4 positionOS : POSITION; 
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID  // 추가
            };
            
            struct Varyings { 
                float4 positionCS : SV_POSITION; 
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO  // 추가
            };
            
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            float4 _BaseColor;
            
            Varyings vert(Attributes IN) {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);  // 추가
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);  // 추가
                
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }
            
            half4 frag(Varyings IN) : SV_Target { 
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);  // 추가
                
                float4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                return texColor * _BaseColor; 
            }
            ENDHLSL
        }
    }
}