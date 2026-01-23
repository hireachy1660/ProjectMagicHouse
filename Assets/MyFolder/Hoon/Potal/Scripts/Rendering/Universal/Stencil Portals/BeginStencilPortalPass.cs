using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule; // 필수 추가
using UnityEngine.Rendering.Universal;
using VRPortalToolkit.Utilities;

namespace VRPortalToolkit.Rendering.Universal
{
    public class BeginStencilPortalPass : ScriptableRenderPass
    {
        public Material increaseMaterial { get; set; }
        public Material clearDepthMaterial { get; set; }
        public PortalPassNode passNode { get; set; }

        private static readonly Plane[] _planes = new Plane[6];

        // 렌더 그래프용 데이터 구조체
        private class PassData
        {
            public PortalPassNode passNode;
            public Material increaseMaterial;
            public Material clearDepthMaterial;
            public RenderingData renderingData; // 컬링 등에 사용
        }

        public BeginStencilPortalPass(RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques)
        {
            this.renderPassEvent = renderPassEvent;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            // 유니티 6에서는 데이터를 frameData에서 가져옵니다.
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("BeginStencilPortalPass", out var passData))
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

                    PortalPassStack.Push(data.passNode);
                    PortalRenderNode renderNode = data.passNode.renderNode;
                    var cmd = context.cmd; // RasterCommandBuffer 사용

                    Material incMat = renderNode.overrides.portalIncrease ? renderNode.overrides.portalIncrease : data.increaseMaterial;
                    Material clearMat = renderNode.overrides.portalClearDepth ? renderNode.overrides.portalClearDepth : data.clearDepthMaterial;

                    // 1. 매트릭스 설정
                    PortalPassStack.Parent.SetViewAndProjectionMatrices(cmd);

                    // 2. 스텐실 마스킹 (RasterCommandBuffer는 SetGlobalInt를 지원합니다)
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

                    // 3. 컬링 및 이벤트 (기존 로직 유지)
                    PortalRendering.onPreRender?.Invoke(renderNode);
                    foreach (IPortalRenderer renderer in renderNode.renderers)
                        renderer?.PreCull(renderNode);

                    // 뷰포트 설정 등...
                    float width = cameraData.cameraTargetDescriptor.width;
                    float height = cameraData.cameraTargetDescriptor.height;
                    Rect rect = renderNode.cullingWindow.GetRect();
                    data.passNode.viewport = new Rect(rect.x * width, rect.y * height, rect.width * width, rect.height * height);

                    // 4. 카메라 및 컬링 파라미터 업데이트 (유니티 6 방식)
                    cmd.SetGlobalVector(PropertyID.WorldSpaceCameraPos, (Vector3)renderNode.localToWorldMatrix.GetColumn(3));

                    // 주의: context.Submit()은 렌더 그래프에서 자동으로 처리되므로 호출하지 않습니다.
                });
            }
        }

        // 구형 방식 호환성 유지 (필요 시)
        [Obsolete]
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) { }
    }
}