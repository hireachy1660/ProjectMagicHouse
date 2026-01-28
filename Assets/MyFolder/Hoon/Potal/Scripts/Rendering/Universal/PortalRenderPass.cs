using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using VRPortalToolkit.Rendering; // PortalPassNode 참조를 위해 추가

namespace VRPortalToolkit.Rendering.Universal
{
    public abstract class PortalRenderPass : ScriptableRenderPass
    {
        // 모든 자식 패스가 공통으로 사용할 데이터 구조체
        public class PortalPassData
        {
            // 타입을 명확히 PortalPassNode로 고정합니다.
            public PortalPassNode node;
        }

        public PortalRenderPass(RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques) : base()
        {
            this.renderPassEvent = renderPassEvent;
            profilingSampler = new ProfilingSampler(GetType().Name);
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData) { }

        [Obsolete]
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) { }
    }
}