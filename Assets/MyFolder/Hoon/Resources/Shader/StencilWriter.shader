Shader "Custom/URP_StencilWriter"
{
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType"="Transparent" "Queue"="Geometry-1" }
        
        Pass
        {
            ZWrite Off
            ColorMask 0
            
            Stencil
            {
                Ref 1
                Comp Always
                Pass Replace
            }
        }
    }
}