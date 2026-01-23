using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace VRPortalToolkit.Rendering.Universal
{
    public class CompleteStencilPortalPass : ScriptableRenderPass
    {
        public Material decreaseMaterial { get; set; }
        public PortalPassNode passNode { get; set; }

        private class PassData
        {
            public PortalPassNode passNode;
            public Material decreaseMaterial;
        }

        public CompleteStencilPortalPass(RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques)
        {
            this.renderPassEvent = renderPassEvent;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("CompleteStencilPortalPass", out var passData))
            {
                passData.passNode = passNode;
                passData.decreaseMaterial = decreaseMaterial;

                builder.UseTexture(resourceData.activeColorTexture, AccessFlags.Write);
                builder.UseTexture(resourceData.activeDepthTexture, AccessFlags.Write);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    if (data.passNode == null || data.passNode.renderNode == null) return;

                    var cmd = context.cmd;
                    PortalRenderNode renderNode = data.passNode.renderNode;

                    // 1. 매트릭스 설정 (수정된 PortalPassStack 사용)
                    data.passNode.SetViewAndProjectionMatrices(cmd);

                    // 2. 스텐실 감소 로직
                    cmd.SetGlobalInt(PropertyID.PortalStencilRef, renderNode.depth);
                    Material decMat = renderNode.overrides.portalDecrease ? renderNode.overrides.portalDecrease : data.decreaseMaterial;

                    if (decMat != null)
                    {
                        foreach (IPortalRenderer renderer in renderNode.renderers)
                            renderer?.Render(renderNode, cmd, decMat);
                    }

                    // 3. 상태 복구 및 팝
                    if (PortalPassStack.Parent != null)
                        PortalPassStack.Parent.RestoreState(cmd);

                    PortalPassStack.Pop();
                });
            }
        }

        [Obsolete]
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) { }
    }
}