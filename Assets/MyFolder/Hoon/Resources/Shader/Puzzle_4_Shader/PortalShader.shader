Shader "Custom/PortalShader"
{
    Properties
    {
        _LeftTex ("Left Eye Texture", 2D) = "white" {}
        _RightTex ("Right Eye Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            // 유니티 6 XR/VR 기능을 사용하기 위한 필수 키워드
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID // 추가
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 screenPos : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO // VR 눈 인덱스 포함
            };

            sampler2D _LeftTex;
            sampler2D _RightTex;

            Varyings vert (Attributes input)
            {
                Varyings output;
                
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output); // 출력 구조체 초기화

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.screenPos = ComputeScreenPos(output.positionCS);
                
                return output;
            }

            half4 frag (Varyings input) : SV_Target
            {
                // 현재 그리는 눈의 인덱스 가져오기 (0: 왼쪽, 1: 오른쪽)
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                float2 uv = input.screenPos.xy / input.screenPos.w;

                // 인덱스에 따라 텍스처 선택
                if (unity_StereoEyeIndex == 0)
                {
                    return tex2D(_LeftTex, uv);
                }
                else
                {
                    return tex2D(_RightTex, uv);
                }
            }
            ENDHLSL
        }
    }
}