Shader "Custom/URP_StencilReader_Fixed"
{

    Properties
    {
        _StencilComp("Stencil Comparison", Float) = 3
        _BaseColor("Base Color", Color) = (1,1,1,1)
        _Brightness("Brightness Manual Adjust", Range(0, 2)) = 1.0 // 수동 밝기 조절 추가
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Stencil
            {
                Ref 1
                Comp [_StencilComp] // 변수로 제어할 수 있게 수정
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
            };

            half4 _BaseColor;
            half _Brightness;

            Varyings vert(Attributes IN) {
                Varyings OUT;
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionCS = TransformWorldToHClip(OUT.positionWS);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target {
                // 현실 세계와 동일한 조명 데이터 가져오기
                float4 shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
                Light light = GetMainLight(shadowCoord);
                
                // 표준 URP 조명 계산
                half3 ambient = SampleSH(IN.normalWS);
                half3 diffuse = light.color * saturate(dot(IN.normalWS, light.direction)) * light.shadowAttenuation;
                
                // 최종 컬러에 수동 조절값(_Brightness)을 곱해 확실하게 제어합니다.
                half3 finalColor = _BaseColor.rgb * (diffuse + ambient) * _Brightness;
                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}