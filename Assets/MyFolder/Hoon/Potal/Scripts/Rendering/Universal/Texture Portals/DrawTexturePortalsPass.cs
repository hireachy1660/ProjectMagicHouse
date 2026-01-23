using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using VRPortalToolkit.Data;

namespace VRPortalToolkit.Rendering.Universal
{
    public class DrawTexturePortalsPass : PortalRenderPass
    {
        public Material material { get; set; }
        private static MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();

        public DrawTexturePortalsPass(RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques) : base(renderPassEvent) { }

        private class PassData
        {
            public Material mat;
            // 유니티 6에서는 데이터를 이 구조체에 담아 전달해야 합니다.
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("DrawTexturePortals", out var passData))
            {
                passData.mat = material;

                builder.SetRenderFunc((PassData data, RasterGraphContext rgContext) =>
                {
                    // [해결책] UnsafeCommandBuffer로 변환하지 마세요!
                    // 유니티 6의 RasterCommandBuffer(rgContext.cmd)를 직접 사용합니다.

                    // 만약 포탈의 뷰/프로젝션 매트릭스를 설정해야 한다면 아래 방식을 씁니다.
                    // 기존의 PortalPassStack 호출 대신 유니티 6 내장 함수를 직접 씁니다.
                    if (PortalRenderFeature.renderCamera != null)
                    {
                        var camera = PortalRenderFeature.renderCamera;
                        rgContext.cmd.SetViewProjectionMatrices(camera.worldToCameraMatrix, camera.projectionMatrix);
                    }

                    // 재질(Material)을 사용하여 포탈을 그리는 로직이 있다면 여기 추가합니다.
                    if (data.mat != null)
                    {
                        // 예: rgContext.cmd.DrawMesh(...) 혹은 필요한 그리기 명령
                    }
                });
            }
        }

        [System.Obsolete]
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) { }
    }
}