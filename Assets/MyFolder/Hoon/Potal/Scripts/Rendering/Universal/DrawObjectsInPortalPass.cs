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
                // 4. 실제 렌더링 명령 예약
                builder.SetRenderFunc((PortalPassData data, RasterGraphContext context) =>
                {
                    // 스택이 비어있으면 렌더링을 스킵합니다.
                    if (PortalPassStack.Current == null) return;

                    var cmd = context.cmd;
                    var currentNode = PortalPassStack.Current;

                    // 매트릭스 설정 (포탈 시점으로 뷰 변환)
                    currentNode.SetViewAndProjectionMatrices(cmd);

                    // 스텐실 참조값 설정
                    // 현재 포탈의 depth를 스텐실 값으로 사용하여, 이전 패스(BeginStencil)에서 그려진 영역에만 그립니다.
                    cmd.SetGlobalInt("_PortalStencilRef", currentNode.renderNode.depth);

                    // [추가 제안] 뷰포트 설정
                    // PortalPassNode에 저장된 viewport가 있다면 적용하여 렌더링 범위를 제한할 수 있습니다.
                    // cmd.SetViewport(currentNode.viewport); 

                    context.cmd.DrawRendererList(rendererList);
                });
            }
        }

        // 구형 Execute는 Obsolete 처리하여 경고를 방지합니다.
        [System.Obsolete]
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) { }
    }
}