using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace VRPortalToolkit.Rendering.Universal
{
    public class DrawDepthOnlyPortalsPass : ScriptableRenderPass
    {
        public PortalPassNode passNode { get; set; }

        private class PassData
        {
            public PortalPassNode passNode;
        }

        public DrawDepthOnlyPortalsPass(RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques)
        {
            this.renderPassEvent = renderPassEvent;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("DrawDepthOnlyPortalsPass", out var passData))
            {
                passData.passNode = passNode;
                builder.UseTexture(resourceData.activeDepthTexture, AccessFlags.Write);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    if (data.passNode == null || data.passNode.renderNode == null) return;

                    var cmd = context.cmd;
                    data.passNode.SetViewAndProjectionMatrices(cmd);

                    foreach (IPortalRenderer renderer in data.passNode.renderNode.renderers)
                        renderer?.RenderDefault(data.passNode.renderNode, cmd);
                });
            }
        }

        [Obsolete]
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) { }
    }
}