Shader "Custom/StencilWrite" {
    SubShader {
        Tags { "RenderType"="Opaque" "Queue"="Geometry-1" }
        Pass {
            Stencil {
                Ref 1
                Comp Always
                Pass Replace
            }
            ColorMask 0 // 화면에 아무 색도 칠하지 않음(투명)
            ZWrite Off  // 뒤에 있는 물체를 가리지 않음
        }
    }
}