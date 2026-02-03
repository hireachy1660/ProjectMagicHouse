using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

namespace VRPortalToolkit.Rendering.Universal
{
    public class CompleteUndoStencilPortalPass : ScriptableRenderPass
    {
        public Material decreaseMaterial { get; set; }
        public PortalPassNode passNode { get; set; }

        private class PassData
        {
            public PortalPassNode passNode;
            public Material decreaseMaterial;
        }

        public CompleteUndoStencilPortalPass(RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques)
        {
            this.renderPassEvent = renderPassEvent;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("CompleteUndoStencilPortalPass", out var passData))
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

                    // 부모 노드 기준 매트릭스 설정 (Undo이므로 부모로 돌아감)
                    if (PortalPassStack.Parent != null)
                        PortalPassStack.Parent.SetViewAndProjectionMatrices(cmd);

                    // 스텐실 감소 로직
                    cmd.SetGlobalInt(PropertyID.PortalStencilRef, renderNode.depth);
                    Material decMat = renderNode.overrides.portalDecrease ? renderNode.overrides.portalDecrease : data.decreaseMaterial;

                    if (decMat != null)
                    {
                        foreach (IPortalRenderer renderer in renderNode.renderers)
                            renderer?.Render(renderNode, cmd, decMat);
                    }

                    // 스택 팝 및 상태 복구
                    PortalPassStack.Pop();
                    if (PortalPassStack.Current != null)
                        PortalPassStack.Current.RestoreState(cmd);
                });
            }
        }

        [System.Obsolete]
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) { }
    }
}