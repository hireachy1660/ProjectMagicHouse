using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using VRPortalToolkit.Rendering.Universal;

namespace VRPortalToolkit.Rendering.Universal
{
    /// <summary>
    /// 텍스처 기반 포탈의 렌더링 프로세스를 완료하는 렌더 패스입니다. (Unity 6 대응)
    /// </summary>
    public class CompleteTexturePortalPass : ScriptableRenderPass
    {
        private class PassData { }

        public CompleteTexturePortalPass(RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques)
        {
            this.renderPassEvent = renderPassEvent;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            using (var builder = renderGraph.AddUnsafePass<PassData>("CompleteTexturePortalPass", out var passData))
            {
                builder.SetRenderFunc((PassData data, UnsafeGraphContext context) =>
                {
                    // [중요] UnsafeCommandBuffer에서 내부 CommandBuffer를 안전하게 추출합니다.
                    CommandBuffer cmd = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);

                    PortalPassNode passNode = PortalPassStack.Pop();
                    if (passNode == null) return;

                    PortalRenderNode renderNode = passNode.renderNode;

                    // 1. 그림자 리소스 정리 (이제 CommandBuffer 타입이 일치하여 에러가 없습니다)
                    if (passNode.mainLightShadowCasterPass != null)
                        passNode.mainLightShadowCasterPass.OnPortalCleanup(cmd);

                    if (passNode.additionalLightsShadowCasterPass != null)
                        passNode.additionalLightsShadowCasterPass.OnPortalCleanup(cmd);

                    // 2. 이벤트 트리거
                    if (renderNode != null && renderNode.renderers != null)
                    {
                        foreach (IPortalRenderer renderer in renderNode.renderers)
                            renderer?.PostRender(renderNode);

                        PortalRendering.onPostRender?.Invoke(renderNode);
                    }

                    PortalPassGroupPool.Release(passNode);

                    // 3. 부모 상태로 복구
                    if (PortalPassStack.Current != null)
                    {
                        // PortalPassStack.cs의 메서드들이 CommandBuffer를 받도록 수정되었으므로 직접 호출합니다.
                        PortalPassStack.Current.RestoreState(cmd);
                        PortalPassStack.Current.SetViewAndProjectionMatrices(cmd);
                    }
                });
            }
        }

        [System.Obsolete]
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) { }
    }
}