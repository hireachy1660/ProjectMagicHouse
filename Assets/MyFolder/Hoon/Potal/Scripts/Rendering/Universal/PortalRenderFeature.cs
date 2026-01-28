using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using VRPortalToolkit.Data;
// [중요] PortalMeshRenderer를 인식하기 위해 네임스페이스 추가
using VRPortalToolkit.Rendering;

namespace VRPortalToolkit.Rendering.Universal
{
    public partial class PortalRenderFeature : ScriptableRendererFeature
    {
        public enum RenderMode { RenderTexture = 0, StencilEarly = 1, Stencil = 2, StencilLate = 3 }

        [Header("Portal Settings")]
        [SerializeField] private RenderMode _renderMode = RenderMode.Stencil;
        [SerializeField] private int _maxDepth = 8;
        [SerializeField] private LayerMask _portalLayerMask = -1;

        [Header("Required Materials")]
        [SerializeField] private Material _portalIncrease;
        [SerializeField] private Material _portalDecrease;

        private PortalBeginPass _beginPass;
        private PortalEndPass _endPass;
        private List<PortalStepPass> _stepPool = new List<PortalStepPass>();
        private int _poolIndex;

        public static Camera renderCamera;

        public override void Create()
        {
            if (renderCamera == null)
            {
                GameObject go = new GameObject("[Portal Render Camera]") { hideFlags = HideFlags.HideAndDontSave };
                renderCamera = go.AddComponent<Camera>();
                renderCamera.enabled = false;
                renderCamera.clearFlags = CameraClearFlags.SolidColor;
                renderCamera.backgroundColor = new Color(0, 0, 0, 0);
            }

            _beginPass = new PortalBeginPass { renderPassEvent = RenderPassEvent.BeforeRenderingOpaques };
            _endPass = new PortalEndPass { renderPassEvent = RenderPassEvent.AfterRenderingTransparents };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            var cameraData = renderingData.cameraData;
            if (cameraData.cameraType == CameraType.Preview || cameraData.cameraType == CameraType.Reflection) return;

            var allRenderers = PortalRendering.GetAllPortalRenderers();

            // 알고리즘 트리를 생성하여 렌더링할 포탈 노드들을 계산
            PortalRenderNode rootNode = PortalAlgorithms.GetTree(
                cameraData.camera,
                cameraData.camera.transform.localToWorldMatrix,
                cameraData.camera.worldToCameraMatrix,
                cameraData.camera.projectionMatrix,
                cameraData.camera.cullingMask,
                0, _maxDepth, 32,
                allRenderers
            );

            if (rootNode == null || rootNode.validChildCount == 0) return;

            renderer.EnqueuePass(_beginPass);
            _poolIndex = 0;
            EnqueueRecursive(renderer, rootNode);
            renderer.EnqueuePass(_endPass);
        }

        private void EnqueueRecursive(ScriptableRenderer renderer, PortalRenderNode parent)
        {
            foreach (var child in parent.children)
            {
                if (!child.isValid) continue;

                if (_poolIndex >= _stepPool.Count) _stepPool.Add(new PortalStepPass());
                var pass = _stepPool[_poolIndex++];

                pass.node = child;
                pass.increaseMaterial = _portalIncrease;
                pass.decreaseMaterial = _portalDecrease;
                pass.renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;

                renderer.EnqueuePass(pass);

                if (child.validChildCount > 0)
                    EnqueueRecursive(renderer, child);
            }
        }

        private class PortalStepPass : ScriptableRenderPass
        {
            public PortalRenderNode node;
            public Material increaseMaterial;
            public Material decreaseMaterial;

            private class PassData
            {
                public PortalRenderNode node;
                public Material incMat;
                public Material decMat;
                public Matrix4x4 originalView;
                public Matrix4x4 originalProj;
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                if (node == null || node.renderer == null) return;

                var resourceData = frameData.Get<UniversalResourceData>();
                var cameraData = frameData.Get<UniversalCameraData>();

                using (var builder = renderGraph.AddRasterRenderPass<PassData>($"Portal_Step_D{node.depth}", out var passData))
                {
                    passData.node = node;
                    passData.incMat = increaseMaterial;
                    passData.decMat = decreaseMaterial;

                    // 현재 카메라의 원래 행렬 저장
                    passData.originalView = cameraData.GetViewMatrix();
                    passData.originalProj = cameraData.GetProjectionMatrix();

                    builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.Write);
                    builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, AccessFlags.Write);

                    // [중요] 포탈 면이 화면 구석에 있어도 강제로 렌더링하도록 설정
                    builder.AllowPassCulling(false);
                    builder.AllowGlobalStateModification(true);

                    // PortalRenderFeature.cs 내부의 PortalStepPass 수정
                    builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                    {
                        var cmd = context.cmd;
                        var meshRenderer = data.node.renderer as PortalMeshRenderer;
                        if (meshRenderer == null) return;

                        // 1. 전역 변수 일치화 (매우 중요)
                        cmd.SetGlobalInt("_PortalStencilRef", data.node.depth);

                        // 2. 포탈 입구 영역 마킹 (Stencil 값을 1, 2, 3... 으로 올림)
                        if (data.incMat != null)
                        {
                            // 셰이더가 사용하는 이름을 강제로 직접 설정
                            data.incMat.SetInt("_PortalStencilRef", data.node.depth);
                            data.incMat.SetInt("_StencilComp", (int)CompareFunction.Always);
                            data.incMat.SetInt("_StencilOp", (int)StencilOp.Replace);
                            cmd.DrawMesh(meshRenderer.filter.sharedMesh, meshRenderer.transform.localToWorldMatrix, data.incMat);
                        }

                        // 3. 시점 전환
                        cmd.SetViewProjectionMatrices(data.node.worldToCameraMatrix, data.node.projectionMatrix);

                        // 4. 내부 렌더링 (Shader Graph 물체들이 스텐실을 무시할 경우를 대비)
                        // 현재 Shader Graph는 스텐실을 지원하지 않으므로, 
                        // 여기서 그리는 모든 물체는 스텐실 없이 허공에 보일 가능성이 큽니다.
                        data.node.renderer.Render(data.node, cmd, null);

                        // 5. 복구
                        cmd.SetViewProjectionMatrices(data.originalView, data.originalProj);
                    });
                }
            }
        }

        private class PortalBeginPass : ScriptableRenderPass
        {
            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                using (var builder = renderGraph.AddRasterRenderPass<NoData>("Portal_Setup_Begin", out _))
                {
                    builder.SetRenderFunc((NoData data, RasterGraphContext context) => {
                        Shader.SetGlobalInt("_PortalStencilRef", 0);
                    });
                }
            }
            [Obsolete] public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) { }
        }

        private class PortalEndPass : ScriptableRenderPass
        {
            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                using (var builder = renderGraph.AddRasterRenderPass<NoData>("Portal_Cleanup_End", out _))
                {
                    builder.SetRenderFunc((NoData data, RasterGraphContext context) => {
                        Shader.SetGlobalInt("_PortalStencilRef", 0);
                    });
                }
            }
            [Obsolete] public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) { }
        }

        private class NoData { }

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