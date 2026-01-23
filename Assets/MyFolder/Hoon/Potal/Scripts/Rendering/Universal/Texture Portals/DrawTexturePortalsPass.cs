using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using VRPortalToolkit.Data;
using VRPortalToolkit.Rendering;

namespace VRPortalToolkit.Rendering.Universal
{
    public class DrawTexturePortalsPass : PortalRenderPass
    {
        private static MaterialPropertyBlock propertyBlock;
        public Material material { get; set; }

        public DrawTexturePortalsPass(RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques) : base(renderPassEvent)
        {
            if (propertyBlock == null) propertyBlock = new MaterialPropertyBlock();
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            using (var builder = renderGraph.AddRasterRenderPass<PortalPassData>(profilingSampler.name, out var passData))
            {
                // 현재 스택의 노드를 데이터로 전달
                passData.node = PortalPassStack.Current;

                builder.SetRenderFunc((PortalPassData data, RasterGraphContext context) =>
                {
                    // 1. 데이터 검증
                    if (data.node == null || data.node.renderNode == null) return;

                    var cmd = context.cmd;

                    // 2. 행렬 설정 에러 해결 (CS1061)
                    // PortalPassNode에 직접 행렬이 없다면 PortalPassStack의 정적 메서드를 사용하거나
                    // 아래와 같이 현재 스택의 행렬을 직접 주입합니다.
                    PortalPassStack.Current.SetViewAndProjectionMatrices(cmd);

                    // 3. 자식 포탈 렌더링
                    foreach (var childNode in data.node.renderNode.children)
                    {
                        if (childNode.isValid && RenderPortalsBuffer.TryGetBuffer(childNode, out RenderPortalsBuffer buffer))
                        {
                            propertyBlock.SetTexture("_MainTex", buffer.texture);

                            foreach (var portalRenderer in childNode.renderers)
                            {
                                // 4. RasterCommandBuffer 변환 에러 해결 (CS1503)
                                // 유니티 6에서는 RasterCommandBuffer가 대부분의 기능을 수행하지만,
                                // 기존 IPortalRenderer 인터페이스가 CommandBuffer를 요구한다면
                                // 아래와 같이 암시적 형변환을 피하고 직접 전달합니다.
                                portalRenderer.Render(childNode, cmd, material, propertyBlock);
                            }
                        }
                    }
                });
            }
        }
    }
}