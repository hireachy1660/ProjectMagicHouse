Shader "Custom/StencilMask"
{
    SubShader
    {
        // 불투명 단계에서 그리되, 일반 물체(2000)보다 미세하게 먼저 그립니다.
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" "Queue"="Geometry-1" }
        
        Pass
        {
            // [핵심] 화면에 아무 색상도 그리지 않음
            ColorMask 0
            // [핵심] 깊이 버퍼를 기록하지 않음 (뒤의 풍경을 가리지 않기 위해)
            ZWrite Off
            
            // 스텐실 설정 (Renderer Feature에서 덮어쓰겠지만 안전을 위해 기입)
            Stencil
            {
                Ref 20          // 팀원과 상의한 ID 값 
                Comp Always     // 무조건 통과하여 기록 
                Pass Replace    // 버퍼의 값을 Ref 값으로 교체 
            }
        }
    }
}