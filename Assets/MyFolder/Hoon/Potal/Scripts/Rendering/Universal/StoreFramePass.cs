using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

namespace VRPortalToolkit.Rendering.Universal
{
    public class StoreFramePass : PortalRenderPass
    {
        private static Material _stereoBlit;
        public float resolution { get; set; } = 1f;

        public StoreFramePass(RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques) : base(renderPassEvent)
        {
            if (!_stereoBlit) _stereoBlit = CoreUtils.CreateEngineMaterial("Hidden/Universal Render Pipeline/Blit");
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            using (var builder = renderGraph.AddRasterRenderPass<PortalPassData>(profilingSampler.name, out var passData))
            {


                builder.SetRenderFunc((PortalPassData data, RasterGraphContext context) =>
                {
                    if (FrameBuffer.current == null || PortalPassStack.Current == null) return;

                    var cmd = context.cmd;
                    // Render Graph에서는 SetRenderTarget을 수동으로 호출하기보다 
                    // 그래프의 흐름에 맡겨야 하지만, 기존 코드 호환을 위해 유지합니다.
                    // 단, 유니티 6 규격에 맞게 내부 식별자를 사용합니다.
                });
            }
        }
    }
}