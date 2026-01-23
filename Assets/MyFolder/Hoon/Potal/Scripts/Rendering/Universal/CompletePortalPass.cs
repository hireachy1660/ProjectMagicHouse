using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;
using VRPortalToolkit.Rendering.Universal;

namespace VRPortalToolkit
{
    public class CompletePortalPass : PortalRenderPass
    {
        public CompletePortalPass(RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques) : base(renderPassEvent) { }

        private class PassData { }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("CompletePortalPass", out var passData))
            {
                builder.SetRenderFunc((PassData data, RasterGraphContext rgContext) =>
                {
                    PortalPassStack.Clear();
                });
            }
        }

        [System.Obsolete]
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) { }
    }
}