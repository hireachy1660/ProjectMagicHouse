using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace VRPortalToolkit.Rendering.Universal
{
    /// <summary>
    /// Render pass that draws the skybox for portal rendering.
    /// Updated for Unity 6 (6000.3.x)
    /// </summary>
    public class DrawSkyboxInPortalPass : PortalRenderPass
    {
        public DrawSkyboxInPortalPass(RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques) : base(renderPassEvent) { }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.camera.clearFlags != CameraClearFlags.Skybox)
                return;

            CommandBuffer cmd = CommandBufferPool.Get();

            // Unity 6 대응: renderingData.cameraData.xr 인터페이스 사용
            var xrData = renderingData.cameraData.xr;

            // xrData 자체가 null이 아니면 활성화된 것으로 간주하거나 enabled를 체크합니다.
            bool isXrActive = xrData != null && xrData.enabled;

            // singlePassStereoRendering 대신 singlePassEnabled를 사용합니다.
            bool isSinglePass = isXrActive && xrData.singlePassEnabled;

            {
                Camera renderCamera = PortalRenderFeature.renderCamera;
                PortalRenderNode renderNode = PortalPassStack.Current.renderNode;

                // Viewport 설정
                cmd.SetViewport(new Rect(0f, 0f, renderingData.cameraData.cameraTargetDescriptor.width, renderingData.cameraData.cameraTargetDescriptor.height));

                if (isXrActive)
                {
                    // Single Pass Instanced (기존 != MultiPass 대응)
                    if (isSinglePass)
                    {
                        renderCamera.SetStereoProjectionMatrix(Camera.StereoscopicEye.Left, renderingData.cameraData.GetProjectionMatrix(0));
                        renderCamera.SetStereoViewMatrix(Camera.StereoscopicEye.Left, renderNode.GetStereoViewMatrix(0));
                        renderCamera.SetStereoProjectionMatrix(Camera.StereoscopicEye.Right, renderingData.cameraData.GetProjectionMatrix(1));
                        renderCamera.SetStereoViewMatrix(Camera.StereoscopicEye.Right, renderNode.GetStereoViewMatrix(1));

                        // Unity 6 대응: 시스템 지원 여부에 따라 모드 자동 선택
                        var stereoMode = SystemInfo.supportsMultiview ? SinglePassStereoMode.Multiview : SinglePassStereoMode.Instancing;
                        cmd.SetSinglePassStereo(stereoMode);

                        context.ExecuteCommandBuffer(cmd);
                        cmd.Clear();

                        context.DrawSkybox(renderCamera);

                        cmd.SetSinglePassStereo(SinglePassStereoMode.None);
                        context.ExecuteCommandBuffer(cmd);
                    }
                    else // Multi-pass 대응
                    {
                        context.ExecuteCommandBuffer(cmd);

                        renderCamera.projectionMatrix = renderingData.cameraData.GetProjectionMatrix(0);
                        renderCamera.worldToCameraMatrix = renderNode.worldToCameraMatrix;

                        context.DrawSkybox(renderCamera);
                    }
                }
                else // Non-XR (일반 카메라)
                {
                    context.ExecuteCommandBuffer(cmd);

                    renderCamera.projectionMatrix = renderingData.cameraData.camera.projectionMatrix;
                    renderCamera.worldToCameraMatrix = renderNode.worldToCameraMatrix;

                    context.DrawSkybox(renderCamera);
                }
            }

            CommandBufferPool.Release(cmd);
        }
    }
}