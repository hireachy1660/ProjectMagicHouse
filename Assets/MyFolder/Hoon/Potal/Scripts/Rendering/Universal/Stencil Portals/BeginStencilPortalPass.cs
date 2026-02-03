using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;
using VRPortalToolkit.Utilities;

namespace VRPortalToolkit.Rendering.Universal
{
    public class BeginStencilPortalPass : ScriptableRenderPass
    {
        public Material increaseMaterial { get; set; }
        public Material clearDepthMaterial { get; set; }
        public PortalPassNode passNode { get; set; }

        private class PassData
        {
            public PortalPassNode passNode;
            public Material increaseMaterial;
            public Material clearDepthMaterial;
        }

        public BeginStencilPortalPass(RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques)
        {
            this.renderPassEvent = renderPassEvent;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {


            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("BeginStencilPortalPass", out var passData))
            {
                passData.passNode = passNode;
                passData.increaseMaterial = increaseMaterial;
                passData.clearDepthMaterial = clearDepthMaterial;

                // [중요] 유니티 6: 렌더 타겟에 대한 쓰기 권한 명시
                builder.UseTexture(resourceData.activeColorTexture, AccessFlags.Write);
                builder.UseTexture(resourceData.activeDepthTexture, AccessFlags.Write);


                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    if (data.passNode == null || data.passNode.renderNode == null) return;

                    var cmd = context.cmd;
                    PortalRenderNode renderNode = data.passNode.renderNode;

                    Debug.Log($"<color=green>[Portal Stencil]</color> Setting Stencil Ref: {renderNode.depth}");

                    // 1. 포탈 스택 설정 및 매트릭스 적용
                    PortalPassStack.Push(data.passNode);
                    data.passNode.SetViewAndProjectionMatrices(cmd);

                    Debug.Log($"<color=green>[Stencil]</color> Depth: {renderNode.depth} 준비 중");
         

                    // 2. 쉐이더에 스텐실 값 전달 (회색 면 방지의 핵심)
                    cmd.SetGlobalInt("_PortalStencilRef", renderNode.depth);
                    cmd.SetGlobalInt(PropertyID.PortalStencilRef, renderNode.depth);

                    // 3. 포탈 면 렌더링 (스텐실 마스크 생성)
                    Material incMat = renderNode.overrides.portalIncrease ? renderNode.overrides.portalIncrease : data.increaseMaterial;
                    if (incMat != null)
                    {
                        foreach (IPortalRenderer renderer in renderNode.renderers)
                            renderer?.Render(renderNode, cmd, incMat);
                    }

                    // 4. 깊이 버퍼 클리어
                    Material clearMat = renderNode.overrides.portalClearDepth ? renderNode.overrides.portalClearDepth : data.clearDepthMaterial;
                    if (clearMat != null)
                    {
                        foreach (IPortalRenderer renderer in renderNode.renderers)
                            renderer?.Render(renderNode, cmd, clearMat);
                    }
                });
            }
        }
    }
}