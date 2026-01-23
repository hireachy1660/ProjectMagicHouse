using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using VRPortalToolkit.Rendering.Universal;
using VRPortalToolkit.Rendering;

namespace VRPortalToolkit
{
    public class BeginPortalPass : PortalRenderPass
    {
        public PortalPassNode portalPassNode { get; set; }
        public BeginPortalPass(RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques) : base(renderPassEvent) { }

        private class PassData { public PortalPassNode node; }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("BeginPortalPass", out var passData))
            {
                passData.node = portalPassNode;
                builder.SetRenderFunc((PassData data, RasterGraphContext rgContext) =>
                {
                    PortalPassStack.Clear();
                    PortalPassStack.Push(data.node);
                });
            }
        }

        [System.Obsolete]
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) { }
    }
}