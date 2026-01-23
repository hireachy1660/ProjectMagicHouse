using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

namespace VRPortalToolkit.Rendering.Universal
{
    public partial class PortalRenderFeature : ScriptableRendererFeature
    {
        public enum PortalRenderMode { Stencil, StencilEarly, RenderTexture }

        public static Camera renderCamera;

        [Header("Portal Settings")]
        [SerializeField] private PortalRenderMode _renderMode = PortalRenderMode.Stencil;
        [SerializeField] private int _maxDepth = 2;
        [SerializeField] private LayerMask _portalLayerMask = -1;

        private PortalBeginPass beginPass;
        private PortalEndPass endPass;
        private List<PortalRenderPass> pool = new List<PortalRenderPass>();
        private int poolIndex;
        private bool _isDirty = true;

        public override void Create()
        {
            if (renderCamera == null)
            {
                GameObject go = new GameObject("Portal Render Camera") { hideFlags = HideFlags.HideAndDontSave };
                renderCamera = go.AddComponent<Camera>();
                renderCamera.enabled = false;
            }

            beginPass = new PortalBeginPass { renderPassEvent = RenderPassEvent.BeforeRenderingOpaques };
            endPass = new PortalEndPass { renderPassEvent = RenderPassEvent.AfterRenderingTransparents };
            _isDirty = false;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            var cameraData = renderingData.cameraData;
            Camera camera = cameraData.camera;

            if (camera.cameraType == CameraType.Preview || camera.cameraType == CameraType.Reflection) return;
            if (_isDirty) Create();

            // [STEP 1] 시스템 진입 확인
            Debug.Log($"<color=cyan>[Portal]</color> AddRenderPasses 시작 (카메라: {camera.name})");

            PortalRenderNode rootNode = PortalRenderNode.Get(camera);
            rootNode.worldToCameraMatrix = camera.worldToCameraMatrix;
            rootNode.projectionMatrix = camera.projectionMatrix;
            rootNode.cullingMask = camera.cullingMask;

            var allRenderers = GameObject.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            bool portalFound = false;

            renderer.EnqueuePass(beginPass);

            foreach (var mono in allRenderers)
            {
                if (mono is IPortalRenderer portalRenderer && ((1 << mono.gameObject.layer) & _portalLayerMask) != 0)
                {
                    // [STEP 2] 포탈 오브젝트 탐지 확인
                    Debug.Log($"<color=yellow>[Portal Found]</color> '{mono.name}' 오브젝트를 포탈로 인식했습니다.");

                    var childNode = rootNode.GetOrAddChild(portalRenderer);
                    if (childNode != null)
                    {
                        childNode.isValid = true;
                        childNode.ComputeMaskAndMatrices();
                        portalFound = true;

                        poolIndex = 0;
                        if (_renderMode == PortalRenderMode.RenderTexture)
                        {
                            EnqueueRenderNodes(renderer, ref renderingData, childNode);
                        }
                        else
                        {
                            int stencilOrder = (_renderMode == PortalRenderMode.StencilEarly) ? 1 : 0;
                            EnqueueStencilNodes(renderer, ref renderingData, childNode, stencilOrder);
                        }

                        // [STEP 3] 패스 등록 확인
                        Debug.Log($"<color=green>[Portal Pass Enqueued]</color> {mono.name}에 대한 렌더 패스가 등록되었습니다 (Depth: {childNode.depth})");
                    }
                    else
                    {
                        Debug.LogWarning($"<color=orange>[Portal Warning]</color> {mono.name}의 노드 생성에 실패했습니다 (Culling 또는 Window 문제)");
                    }
                }
            }

            if (!portalFound)
            {
                // 포탈을 못 찾은 이유가 레이어 마스크 때문일 수 있으므로 힌트 출력
                Debug.Log("<color=white>[Portal Info]</color> 현재 씬에서 유효한 포탈을 찾지 못했습니다. 레이어 마스크 설정을 확인하세요.");
                rootNode.Dispose();
            }

            renderer.EnqueuePass(endPass);
        }

        private void EnqueueRenderNodes(ScriptableRenderer renderer, ref RenderingData renderingData, PortalRenderNode node)
        {
            var pass = GetOrCreatePass();
            pass.node = node;
            renderer.EnqueuePass(pass);
        }

        private void EnqueueStencilNodes(ScriptableRenderer renderer, ref RenderingData renderingData, PortalRenderNode node, int order)
        {
            var pass = GetOrCreatePass();
            pass.node = node;
            renderer.EnqueuePass(pass);
        }

        private PortalRenderPass GetOrCreatePass()
        {
            if (poolIndex < pool.Count) return pool[poolIndex++];
            var pass = new PortalRenderPass();
            pool.Add(pass);
            poolIndex++;
            return pass;
        }

        // --- 내부 패스 클래스 (Render Graph 대응) ---

        private class PortalBeginPass : ScriptableRenderPass
        {
            private class PassData { }
            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                using (var builder = renderGraph.AddRasterRenderPass<PassData>("Portal_Begin", out _))
                {
                    builder.SetRenderFunc((PassData data, RasterGraphContext context) => { });
                }
            }
            [Obsolete] public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) { }
        }

        private class PortalRenderPass : ScriptableRenderPass
        {
            public PortalRenderNode node;
            private class PassData { public PortalRenderNode node; }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                var resourceData = frameData.Get<UniversalResourceData>();
                string debugName = $"Portal_Step_Depth_{node?.depth ?? 0}";

                using (var builder = renderGraph.AddRasterRenderPass<PassData>(debugName, out var passData))
                {
                    passData.node = node;

                    // 텍스처 사용 등록
                    builder.UseTexture(resourceData.activeColorTexture, AccessFlags.Write);
                    builder.UseTexture(resourceData.activeDepthTexture, AccessFlags.Write);

                    builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                    {
                        // [DEBUG] 실제 렌더링 명령 시점 로그
                        Debug.Log($"<color=green>[Portal Draw]</color> Render 호출 (Depth: {data.node?.depth})");

                        if (data.node != null && data.node.renderer != null)
                        {
                            // [핵심] context.cmd는 RasterCommandBuffer입니다.
                            // IPortalRenderer.Render가 RasterCommandBuffer를 받도록 수정되었으므로 에러가 나지 않습니다.
                            data.node.renderer.Render(data.node, context.cmd, null);
                        }
                        else
                        {
                            Debug.LogWarning("<color=yellow>[Portal Warning]</color> 노드가 비어있거나 렌더러가 없습니다.");
                        }
                    });
                }
            }
            [Obsolete] public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) { }
        }

        private class PortalEndPass : ScriptableRenderPass
        {
            private class PassData { }
            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                using (var builder = renderGraph.AddRasterRenderPass<PassData>("Portal_End", out _))
                {
                    builder.SetRenderFunc((PassData data, RasterGraphContext context) => { });
                }
            }
            [Obsolete] public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) { }
        }

        protected override void Dispose(bool disposing)
        {
            if (renderCamera != null)
            {
                if (Application.isPlaying) Destroy(renderCamera.gameObject);
                else DestroyImmediate(renderCamera.gameObject);
            }
        }
    }
}