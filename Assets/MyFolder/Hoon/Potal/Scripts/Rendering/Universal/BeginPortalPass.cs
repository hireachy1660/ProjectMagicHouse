using UnityEngine;
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

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            using (var builder = renderGraph.AddRasterRenderPass<PortalPassData>(profilingSampler.name, out var passData))
            {
                passData.node = portalPassNode;

                builder.SetRenderFunc((PortalPassData data, RasterGraphContext context) =>
                {
                    PortalPassStack.Clear();
                    if (data.node != null)
                        PortalPassStack.Push(data.node);
                });
            }
        }
    }
}