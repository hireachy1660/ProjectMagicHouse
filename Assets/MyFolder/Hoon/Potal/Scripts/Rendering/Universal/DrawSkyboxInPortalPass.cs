using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

namespace VRPortalToolkit.Rendering.Universal
{
    public class DrawSkyboxInPortalPass : PortalRenderPass
    {
        public DrawSkyboxInPortalPass(RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques) : base(renderPassEvent) { }

        private class PassData { public Camera camera; }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("DrawSkyboxInPortal", out var passData))
            {
                passData.camera = PortalRenderFeature.renderCamera;
                builder.SetRenderFunc((PassData data, RasterGraphContext rgContext) =>
                {
                    // 유니티 6: RasterCommandBuffer에서 직접 DrawSkybox가 안되므로
                    // 하이레벨 API인 DrawRendererList 등을 쓰거나 아래 방식을 사용합니다.
                    if (data.camera != null)
                    {
                        // 이 부분은 유니티 6에서 자동으로 처리되도록 Enqueue 시점을 조정하는 것이 권장됩니다.
                    }
                });
            }
        }

        [System.Obsolete]
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) { }
    }
}