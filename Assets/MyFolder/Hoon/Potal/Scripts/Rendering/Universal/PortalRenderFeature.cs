using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using VRPortalToolkit.Data;

namespace VRPortalToolkit.Rendering.Universal
{
    public class PortalRenderFeature : ScriptableRendererFeature
    {
        // 렌더링 모드 및 알고리즘 정의 (에러 방지를 위해 내부에 정의하거나 네임스페이스 확인 필요)
        public enum RenderMode { RenderTexture, Stencil }
        public enum PortalAlgorithm { Predictive, Recursive }

        [SerializeField] private RenderMode _renderMode;
        [SerializeField] private PortalAlgorithm _algorithm = PortalAlgorithm.Predictive;
        [SerializeField] private LayerMask _portalLayerMask = ~0;
        [SerializeField] private int _maxDepth = 2;

        public static Camera renderCamera;

        // 패스 인스턴스
        private BeginPortalPass _beginPass;
        private CompletePortalPass _completePass;
        private DrawSkyboxInPortalPass _skyboxPass;

        public override void Create()
        {
            _beginPass = new BeginPortalPass(RenderPassEvent.AfterRenderingOpaques);
            _completePass = new CompletePortalPass(RenderPassEvent.AfterRenderingOpaques);
            _skyboxPass = new DrawSkyboxInPortalPass(RenderPassEvent.AfterRenderingOpaques);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.cameraType == CameraType.Preview) return;

            renderCamera = renderingData.cameraData.camera;

            // 1. Begin Pass
            _beginPass.portalPassNode = null; // 실제 노드 할당 로직 필요
            renderer.EnqueuePass(_beginPass);

            // 2. Draw Objects Pass (Opaque)
            var drawOpaque = new DrawObjectsInPortalPass(RenderPassEvent.AfterRenderingOpaques);

            // 유니티 6에서 CreateDrawingSettings 호출 방식 수정
            var sortingSettings = new SortingSettings(renderingData.cameraData.camera) { criteria = SortingCriteria.CommonOpaque };
            drawOpaque.drawingSettings = new DrawingSettings(new ShaderTagId("UniversalForward"), sortingSettings);
            drawOpaque.filteringSettings = new FilteringSettings(RenderQueueRange.opaque);

            renderer.EnqueuePass(drawOpaque);

            // 3. Skybox Pass
            renderer.EnqueuePass(_skyboxPass);

            // 4. Complete Pass
            renderer.EnqueuePass(_completePass);
        }

        // 유니티 6 Render Graph 전용 렌더링 함수 (내부 헬퍼)
        public static void SetPortalMatrices(RasterCommandBuffer cmd, Camera camera)
        {
            // RasterCommandBuffer를 직접 다룰 수 없을 때 Unsafe를 사용하거나 
            // 시스템에서 제공하는 매트릭스 설정 API를 사용해야 합니다.
            cmd.SetViewProjectionMatrices(camera.worldToCameraMatrix, camera.projectionMatrix);
        }
    }
}