using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

namespace VRPortalToolkit.Rendering.Universal
{
    public class DrawObjectsInPortalPass : PortalRenderPass
    {
        public DrawingSettings drawingSettings { get; set; }
        public FilteringSettings filteringSettings { get; set; }

        public DrawObjectsInPortalPass(RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques) : base(renderPassEvent) { }

        private class PassData { public RendererListHandle rendererList; }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("DrawObjectsInPortal", out var passData))
            {
                UniversalRenderingData universalData = frameData.Get<UniversalRenderingData>();

                // RendererList 생성 (유니티 6 표준)
                var param = new RendererListParams(universalData.cullResults, drawingSettings, filteringSettings);
                RendererListHandle rl = renderGraph.CreateRendererList(param);
                builder.UseRendererList(rl);

                builder.SetRenderFunc((PassData data, RasterGraphContext rgContext) =>
                {
                    // rgContext.cmd는 RasterCommandBuffer이므로 DrawRendererList 사용
                    rgContext.cmd.DrawRendererList(rl);
                });
            }
        }

        [System.Obsolete]
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) { }
    }
}