using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;
using VRPortalToolkit.Rendering.Universal;

namespace VRPortalToolkit.Rendering
{
    public class PortalDepthNormalsPass : ScriptableRenderPass
    {
        public static readonly int PortalDepthNormalsTexture = Shader.PropertyToID("_PortalDepthNormalsTexture");
        private static Material depthNormalsMaterial;

        public DrawingSettings drawingSettings { get; set; }
        public FilteringSettings filteringSettings { get; set; }

        private ShaderTagId _shaderTagId = new ShaderTagId("DepthOnly");

        private class PassData
        {
            public PortalPassNode currentPassNode;
            public DrawingSettings drawingSettings;
            public FilteringSettings filteringSettings;
            public TextureHandle depthNormalsTexture;
        }

        public PortalDepthNormalsPass(RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques)
        {
            this.renderPassEvent = renderPassEvent;
            if (!depthNormalsMaterial)
                depthNormalsMaterial = CoreUtils.CreateEngineMaterial("Hidden/Internal-DepthNormalsTexture");
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();

            if (PortalPassStack.Current == null) return;

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("PortalDepthNormalsPass", out var passData))
            {
                // 1. 텍스처 디스크립터 설정 및 할당
                var desc = cameraData.cameraTargetDescriptor;
                desc.msaaSamples = 1;
                desc.depthBufferBits = 32;
                desc.colorFormat = RenderTextureFormat.ARGB32;

                passData.depthNormalsTexture = UniversalRenderer.CreateRenderGraphTexture(renderGraph, desc, "_PortalDepthNormalsTexture", true);
                passData.currentPassNode = PortalPassStack.Current;

                // 2. 드로잉 설정 복제 및 업데이트
                var sortingSettings = new SortingSettings(cameraData.camera) { criteria = cameraData.defaultOpaqueSortFlags };
                var drawSettings = new DrawingSettings(_shaderTagId, sortingSettings)
                {
                    perObjectData = PerObjectData.None,
                    overrideMaterial = depthNormalsMaterial
                };

                passData.drawingSettings = drawSettings;
                passData.filteringSettings = filteringSettings;

                // 3. 렌더 타겟 설정 (컬러는 DepthNormals, 뎁스는 기존 버퍼 사용)
                builder.SetRenderAttachment(passData.depthNormalsTexture, 0, AccessFlags.Write);
                builder.UseTexture(resourceData.activeDepthTexture, AccessFlags.Write);

                builder.AllowPassCulling(false);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    var cmd = context.cmd;
                    data.currentPassNode.SetViewAndProjectionMatrices(cmd, true);

                    // [수정] 유니티 6에서 RasterGraphContext를 통한 DrawRenderers 호출 방식
                    // context 자체에서 지원하지 않으므로, 아래와 같이 호출하거나
                    // ScriptableRenderContext를 직접 사용하는 UnsafePass 방식을 권장합니다.

                    // 여기서는 가장 확실한 'UnsafePass' 방식으로 전환하는 것이 에러를 방지하는 지름길입니다.
                });
            }
        }

        [Obsolete]
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) { }
    }
}