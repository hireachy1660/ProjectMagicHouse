using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;
using VRPortalToolkit.Rendering.Universal;

namespace VRPortalToolkit
{
    public class CompletePortalPass : PortalRenderPass
    {
        public CompletePortalPass(RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques) : base(renderPassEvent) { }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            using (var builder = renderGraph.AddRasterRenderPass<PortalPassData>(profilingSampler.name, out var passData))
            {
                builder.SetRenderFunc((PortalPassData data, RasterGraphContext context) =>
                {
                    // 렌더링 종료 후 스택 클리어
                    PortalPassStack.Clear();
                });
            }
        }

        [System.Obsolete]
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) { }
    }
}