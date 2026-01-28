using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule; // 필수 추가
using VRPortalToolkit.Rendering;

namespace VRPortalToolkit.Rendering.Universal
{
    public class BeginUndoStencilPortalPass : ScriptableRenderPass
    {
        public Material increaseMaterial { get; set; }
        public Material clearDepthMaterial { get; set; }
        public PortalPassNode passNode { get; set; }

        private static MaterialPropertyBlock propertyBlock;

        // 렌더 그래프 데이터 전달용 클래스
        private class PassData
        {
            public PortalPassNode passNode;
            public Material increaseMaterial;
            public Material clearDepthMaterial;
        }

        public BeginUndoStencilPortalPass(RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques)
        {
            this.renderPassEvent = renderPassEvent;
            if (propertyBlock == null) propertyBlock = new MaterialPropertyBlock();
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("BeginUndoStencilPortalPass", out var passData))
            {
                passData.passNode = passNode;
                passData.increaseMaterial = increaseMaterial;
                passData.clearDepthMaterial = clearDepthMaterial;

                // 렌더 타겟 설정
                builder.UseTexture(resourceData.activeColorTexture, AccessFlags.Write);
                builder.UseTexture(resourceData.activeDepthTexture, AccessFlags.Write);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    if (data.passNode == null || data.passNode.renderNode == null) return;

                    var cmd = context.cmd;
                    PortalPassNode parent = PortalPassStack.Parent;

                    // Pass Group 시작
                    PortalPassStack.Push(data.passNode);
                    PortalRenderNode renderNode = data.passNode.renderNode;

                    Material incMat = renderNode.overrides.portalIncrease ? renderNode.overrides.portalIncrease : data.increaseMaterial;
                    Material clearMat = renderNode.overrides.portalClearDepth ? renderNode.overrides.portalClearDepth : data.clearDepthMaterial;

                    // 1. 매트릭스 설정
                    PortalPassStack.Parent.SetViewAndProjectionMatrices(cmd);

                    // 2. 스텐실 마스킹 로직
                    cmd.SetGlobalInt(PropertyID.PortalStencilRef, renderNode.depth - 1);

                    if (incMat != null)
                    {
                        foreach (IPortalRenderer renderer in renderNode.renderers)
                            renderer?.Render(renderNode, cmd, incMat);
                    }

                    cmd.SetGlobalInt(PropertyID.PortalStencilRef, renderNode.depth);

                    if (clearMat != null)
                    {
                        foreach (IPortalRenderer renderer in renderNode.renderers)
                            renderer?.Render(renderNode, cmd, clearMat);
                    }

                    // 3. 프리 렌더 및 컬링 이벤트
                    PortalRendering.onPreRender?.Invoke(renderNode);
                    foreach (IPortalRenderer renderer in renderNode.renderers)
                        renderer?.PreCull(renderNode);

                    // 4. 뷰포트 설정
                    float width = cameraData.cameraTargetDescriptor.width;
                    float height = cameraData.cameraTargetDescriptor.height;
                    Rect rect = renderNode.cullingWindow.GetRect();
                    data.passNode.viewport = new Rect(rect.x * width, rect.y * height, rect.width * width, rect.height * height);

                    // 5. 상태 복구 (RestoreState)
                    // Note: 유니티 6에서는 렌더링 데이터를 직접 넘기기보다 CommandBuffer를 통한 상태 제어를 권장합니다.
                    if (parent != null)
                    {
                        // 기존의 ref renderingData를 사용하던 방식 대신 cmd를 활용하는 방식으로 호출
                        // RestoreState 내부 구현도 RasterCommandBuffer를 지원하도록 수정되어야 할 수 있습니다.
                        parent.RestoreState(cmd);
                    }

                    // 6. 포스트 컬링 이벤트
                    foreach (IPortalRenderer renderer in renderNode.renderers)
                        renderer?.PostCull(renderNode);
                });
            }
        }

        [Obsolete]
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) { }
    }
}