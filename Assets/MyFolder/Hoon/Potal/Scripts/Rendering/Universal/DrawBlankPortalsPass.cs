using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule; // 추가 필수
using VRPortalToolkit.Data;

namespace VRPortalToolkit.Rendering.Universal
{
    public class DrawBlankPortalsPass : ScriptableRenderPass
    {
        private static MaterialPropertyBlock propertyBlock;
        public Material material { get; set; }

        // 렌더 그래프용 데이터 전달 구조체
        private class PassData
        {
            public PortalRenderNode parentNode;
            public Material material;
            public bool hasFrameBuffer;
            public FrameBuffer currentFrameBuffer;
        }

        public DrawBlankPortalsPass(RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques)
        {
            this.renderPassEvent = renderPassEvent;
            if (propertyBlock == null) propertyBlock = new MaterialPropertyBlock();
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("DrawBlankPortalsPass", out var passData))
            {
                // 현재 데이터 세팅
                passData.parentNode = PortalPassStack.Current?.renderNode;
                passData.material = material;
                passData.currentFrameBuffer = FrameBuffer.current;
                passData.hasFrameBuffer = passData.currentFrameBuffer != null && passData.currentFrameBuffer.texture;

                // 렌더 타겟 설정
                builder.UseTexture(resourceData.activeColorTexture, AccessFlags.Write);
                builder.UseTexture(resourceData.activeDepthTexture, AccessFlags.Write);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    if (data.parentNode == null) return;

                    var cmd = context.cmd; // RasterCommandBuffer 사용

                    // 1. 매트릭스 및 스텐실 설정
                    PortalPassStack.Current.SetViewAndProjectionMatrices(cmd);
                    cmd.SetGlobalInt(PropertyID.PortalStencilRef, PortalPassStack.Current.stateBlock.stencilReference);

                    if (data.hasFrameBuffer)
                        propertyBlock.SetTexture(PropertyID.MainTex, data.currentFrameBuffer.texture);

                    // 2. 자식 노드 순회하며 그리기
                    foreach (PortalRenderNode renderNode in data.parentNode.children)
                    {
                        if (!renderNode.isValid)
                        {
                            if (data.hasFrameBuffer && TryFindAncestorNode(renderNode, data.currentFrameBuffer.rootNode, out PortalRenderNode originalNode))
                            {
                                Material targetMat = renderNode.overrides.portalStereo ? renderNode.overrides.portalStereo : data.material;

                                // 윈도우 계산 (기존 로직 유지)
                                if (renderNode.isStereo)
                                {
                                    UpdateScaleAndTranslation(GetWindow(data.parentNode.GetStereoViewMatrix(0), data.parentNode.GetStereoProjectionMatrix(0), renderNode),
                                        originalNode.GetStereoWindow(0), PropertyID.MainTex_ST);
                                    UpdateScaleAndTranslation(GetWindow(data.parentNode.GetStereoViewMatrix(1), data.parentNode.GetStereoProjectionMatrix(1), renderNode),
                                        originalNode.GetStereoWindow(1), PropertyID.MainTex_ST_2);
                                }
                                else
                                {
                                    UpdateScaleAndTranslation(GetWindow(data.parentNode.worldToCameraMatrix, data.parentNode.projectionMatrix, renderNode),
                                        originalNode.window, PropertyID.MainTex_ST);
                                }

                                // [수정 핵심] RasterCommandBuffer를 지원하는 Render 호출
                                foreach (IPortalRenderer renderer in renderNode.renderers)
                                    renderer?.Render(renderNode, cmd, targetMat, propertyBlock);
                            }
                            else
                            {
                                // 기본 렌더링 호출
                                foreach (IPortalRenderer renderer in renderNode.renderers)
                                    renderer?.RenderDefault(renderNode, cmd);
                            }
                        }
                    }
                });
            }
        }

        [Obsolete]
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) { }

        // --- 이하 헬퍼 함수들은 원본과 동일하게 유지 ---

        private static void UpdateScaleAndTranslation(ViewWindow window, ViewWindow parentWindow, int scaleTranslateID)
        {
            if (parentWindow.xMin < 0f) parentWindow.xMin = 0f;
            if (parentWindow.yMin < 0f) parentWindow.yMin = 0f;
            if (parentWindow.xMax > 1f) parentWindow.xMax = 1f;
            if (parentWindow.yMax > 1f) parentWindow.yMax = 1f;

            Vector2 size = new Vector2(window.xMax - window.xMin, window.yMax - window.yMin),
                parentSize = new Vector2(parentWindow.xMax - parentWindow.xMin, parentWindow.yMax - parentWindow.yMin),
                finalTiling = new Vector2(parentSize.x / size.x, parentSize.y / size.y);

            propertyBlock.SetVector(scaleTranslateID, new Vector4(
                finalTiling.x, finalTiling.y,
                (parentWindow.xMin + parentWindow.xMax - finalTiling.x * (window.xMin + window.xMax)) * 0.5f,
                (parentWindow.yMin + parentWindow.yMax - finalTiling.y * (window.yMin + window.yMax)) * 0.5f
            ));
        }

        protected virtual bool TryFindAncestorNode(PortalRenderNode target, PortalRenderNode root, out PortalRenderNode nextNode)
        {
            PortalRenderNode current = root;
            PortalRenderNode lastValid = null;
            nextNode = null;

            foreach (PortalRenderNode originalNode in GetPath(target))
            {
                if (!TryGetChildWithPortal(current, originalNode.portal, out current))
                {
                    if (current != null && current.portal == originalNode.portal)
                        current = lastValid;
                    else
                    {
                        nextNode = null;
                        return false;
                    }
                }
                if (current.portal == target.portal) lastValid = current;
                if (current.parent.portal == target.portal && current.portal != null)
                    nextNode = current;
            }
            return nextNode != null;
        }

        private bool TryGetChildWithPortal(PortalRenderNode parent, IPortal portal, out PortalRenderNode child)
        {
            if (parent != null)
            {
                foreach (PortalRenderNode childNode in parent.children)
                {
                    if (childNode.portal == portal) { child = childNode; return true; }
                }
            }
            child = null;
            return false;
        }

        private IEnumerable<PortalRenderNode> GetPath(PortalRenderNode node)
        {
            if (node != null && node.parent != null)
            {
                foreach (PortalRenderNode parent in GetPath(node.parent)) yield return parent;
                yield return node;
            }
        }

        private ViewWindow GetWindow(Matrix4x4 view, Matrix4x4 proj, PortalRenderNode node)
        {
            if (node.portal.connected != null)
            {
                Matrix4x4 localToWorld = node.portal.teleportMatrix * node.parent.localToWorldMatrix;
                view = view * node.portal.connected.teleportMatrix;
                node.renderer.TryGetWindow(node, localToWorld.GetColumn(3), view, proj, out ViewWindow window);
                return window;
            }
            return default;
        }
    }
}