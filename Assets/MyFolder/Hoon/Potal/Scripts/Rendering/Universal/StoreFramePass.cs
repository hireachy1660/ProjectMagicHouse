using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace VRPortalToolkit.Rendering.Universal
{
    /// <summary>
    /// Render pass that stores the current frame into a buffer for future portal rendering to create the "infinite" portal effect.
    /// Updated for Unity 6 (6000.3.x) RTHandle System
    /// </summary>
    public class StoreFramePass : PortalRenderPass
    {
        private static Material _stereoBlit;

        /// <summary>
        /// The resolution scale factor for the stored frame texture.
        /// </summary>
        public float resolution { get; set; } = 1f;

        public StoreFramePass(RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques) : base(renderPassEvent)
        {
            if (!_stereoBlit) _stereoBlit = CoreUtils.CreateEngineMaterial("Hidden/Universal Render Pipeline/Blit");
        }

        /// <inheritdoc/>
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            if (resolution > 0)
            {
                cameraTextureDescriptor.depthBufferBits = 0;
                cameraTextureDescriptor.msaaSamples = 1;

                cameraTextureDescriptor.dimension = TextureDimension.Tex2DArray;
                cameraTextureDescriptor.width = Mathf.Max(1, (int)(cameraTextureDescriptor.width * resolution));
                cameraTextureDescriptor.height = Mathf.Max(1, (int)(cameraTextureDescriptor.height * resolution));

                FrameBuffer.current.UpdateTexture(cameraTextureDescriptor);

                // [수정] RenderTargetIdentifier 대신 RTHandle을 사용해야 합니다.
                // FrameBuffer.current.handle이 RTHandle 타입이라고 가정합니다. 
                // 만약 identifier(RenderTargetIdentifier)만 있다면 RTHandles.Alloc을 사용합니다.
                ConfigureTarget(RTHandles.Alloc(FrameBuffer.current.identifier));
            }
        }

        /// <inheritdoc/>
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (FrameBuffer.current == null)
            {
                Debug.LogError("Frame buffer not found!");
                return;
            }

            // [수정] 유니티 6에서는 하드웨어 명령어를 직접 쓰는 대신 RasterCommandBuffer를 권장하지만, 
            // 기존 구조 유지를 위해 CommandBuffer를 사용합니다.
            CommandBuffer cmd = CommandBufferPool.Get();

            {
                FrameBuffer.current.rootNode = PortalPassStack.Current.renderNode.root;

                // [수정] cameraColorTarget은 이제 항상 RTHandle입니다.
                RTHandle source = renderingData.cameraData.renderer.cameraColorTargetHandle;

                // [수정] 렌더 타겟 설정
                cmd.SetRenderTarget(RTHandles.Alloc(FrameBuffer.current.identifier));

                if (PortalPassStack.Current.renderNode.isStereo)
                {
                    cmd.SetGlobalTexture(PropertyID.SourceTex, source);

                    Vector4 scaleBias = new Vector4(1, 1, 0, 0);
                    Vector4 scaleBiasRt = new Vector4(1, 1, 0, 0);
                    cmd.SetGlobalVector(PropertyID.ScaleBias, scaleBias);
                    cmd.SetGlobalVector(PropertyID.ScaleBiasRt, scaleBiasRt);

                    // Procedural 드로잉 시에는 RTHandle 시스템에서도 동일하게 작동합니다.
                    cmd.DrawProcedural(Matrix4x4.identity, _stereoBlit, -1, MeshTopology.Quads, 4, 1, null);
                }
                else
                {
                    // [수정] Blit(source, BuiltinRenderTextureType.CurrentActive) 대신 Blitter 기능을 권장하지만 
                    // 호환성을 위해 유지하되 타입을 맞춰줍니다.
                    cmd.Blit(source, RTHandles.Alloc(BuiltinRenderTextureType.CurrentActive));
                }

                context.ExecuteCommandBuffer(cmd);
            }

            CommandBufferPool.Release(cmd);
        }
    }
}