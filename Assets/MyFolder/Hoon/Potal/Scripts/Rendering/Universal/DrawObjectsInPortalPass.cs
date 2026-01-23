using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

namespace VRPortalToolkit.Rendering.Universal
{
    public class DrawObjectsInPortalPass : PortalRenderPass
    {
        public DrawingSettings drawingSettings { get; set; }
        public FilteringSettings filteringSettings { get; set; }
        public Material overrideMaterial { get; set; }

        public DrawObjectsInPortalPass(RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques) : base(renderPassEvent) { }

        /// <inheritdoc/>
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            // 1. 필요한 프레임 데이터들 추출
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            // cullingResults는 RenderingData 안에 들어있습니다.
            UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();

            // 2. RendererList 생성 (renderingData.cullResults 사용)
            // 유니티 6 정석: renderingData.cullResults를 RendererListParams에 전달
            RendererListParams param = new RendererListParams(renderingData.cullResults, drawingSettings, filteringSettings);
            RendererListHandle rendererList = renderGraph.CreateRendererList(param);

            // 3. 렌더 그래프 패스 추가
            using (var builder = renderGraph.AddRasterRenderPass<PortalPassData>("Draw Objects In Portal", out var passData))
            {
                // 현재 카메라의 텍스처 사용 등록
                builder.UseTexture(resourceData.activeColorTexture, AccessFlags.Write);
                builder.UseTexture(resourceData.activeDepthTexture, AccessFlags.Write);

                // 생성한 렌더러 리스트 사용 등록
                builder.UseRendererList(rendererList);

                // 4. 실제 렌더링 명령 예약
                builder.SetRenderFunc((PortalPassData data, RasterGraphContext context) =>
                {
                    // 포탈 매트릭스 설정 (기존 로직 유지)
                    if (PortalPassStack.Current != null)
                    {
                        PortalPassStack.Current.SetViewAndProjectionMatrices(context.cmd);
                    }

                    // RendererList를 그립니다 (DrawRenderers 대체)
                    context.cmd.DrawRendererList(rendererList);
                });
            }
        }

        // 구형 Execute는 Obsolete 처리하여 경고를 방지합니다.
        [System.Obsolete]
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) { }
    }
}