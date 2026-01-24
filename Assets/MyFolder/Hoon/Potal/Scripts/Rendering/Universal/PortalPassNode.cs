using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using VRPortalToolkit.Utilities;

namespace VRPortalToolkit.Rendering.Universal
{
    public static class PortalPassStack
    {
        private static List<PortalPassNode> portalPassNodes = new List<PortalPassNode>();

        public static void Clear() => portalPassNodes.Clear();

        public static void Push(PortalPassNode node)
        {
            if (node != null) portalPassNodes.Add(node);
        }

        public static PortalPassNode Pop()
        {
            if (portalPassNodes.Count > 0)
            {
                PortalPassNode removed = portalPassNodes[portalPassNodes.Count - 1];
                portalPassNodes.RemoveAt(portalPassNodes.Count - 1);
                return removed;
            }
            return null;
        }

        public static PortalPassNode Parent => (portalPassNodes.Count > 1) ? portalPassNodes[portalPassNodes.Count - 2] : null;
        public static PortalPassNode Current => (portalPassNodes.Count > 0) ? portalPassNodes[portalPassNodes.Count - 1] : null;
    }

    public class PortalPassNode
    {
        public PortalRenderNode renderNode;
        public RenderStateBlock stateBlock;
        public Rect viewport = new Rect(0, 0, 1, 1);

        public MainLightShadowCasterInPortalPass mainLightShadowCasterPass;
        public AdditionalLightsShadowCasterInPortalPass additionalLightsShadowCasterPass;

        private RenderTexture _colorTexture;
        public RenderTexture colorTexture
        {
            get => _colorTexture;
            set
            {
                if (_colorTexture != value)
                {
                    _colorTexture = value;
                    _colorTarget = new RenderTargetIdentifier(_colorTexture, 0, CubemapFace.Unknown, -1);
                }
            }
        }

        private RenderTargetIdentifier _colorTarget;
        public RenderTargetIdentifier colorTarget => _colorTarget;

        private RenderingData _renderingData;
        private Vector4 _lightPos, _lightColor, _lightOcclusionChannel, _additionalLightsCount, _worldSpaceCameraPos;
        private Vector4[] _additionalLightPositions, _additionalLightColors, _additionalLightAttenuations, _additionalLightSpotDirections, _additionalLightOcclusionProbeChannels;

        private static List<Vector4> tempList = new List<Vector4>();

        private static readonly int _unityMatrixV = Shader.PropertyToID("unity_MatrixV");
        private static readonly int _unityMatrixVP = Shader.PropertyToID("unity_MatrixVP");
        private static readonly int _unityMatrixP = Shader.PropertyToID("glstate_matrix_projection");

        // --- 일반 CommandBuffer용 메서드 ---
        public void SetViewAndProjectionMatrices(CommandBuffer cmd, bool setViewport = true)
        {
            if (renderNode == null) return;
            cmd.SetGlobalMatrix(_unityMatrixV, renderNode.worldToCameraMatrix);
            cmd.SetGlobalMatrix(_unityMatrixP, renderNode.projectionMatrix);
            Matrix4x4 viewProj = renderNode.projectionMatrix * renderNode.worldToCameraMatrix;
            cmd.SetGlobalMatrix(_unityMatrixVP, viewProj);
            if (setViewport) cmd.SetViewport(viewport);
        }

        public void RestoreState(CommandBuffer cmd)
        {
            cmd.SetGlobalVector(PropertyID.MainLightPosition, _lightPos);
            cmd.SetGlobalVector(PropertyID.MainLightColor, _lightColor);
            cmd.SetGlobalVector(PropertyID.MainLightOcclusionProbesChannel, _lightOcclusionChannel);
            cmd.SetGlobalVector(PropertyID.AdditionalLightsCount, _additionalLightsCount);
            if (_additionalLightsCount.x != 0f)
            {
                cmd.SetGlobalVectorArray(PropertyID.AdditionalLightsPosition, _additionalLightPositions);
                cmd.SetGlobalVectorArray(PropertyID.AdditionalLightsColor, _additionalLightColors);
                cmd.SetGlobalVectorArray(PropertyID.AdditionalLightsAttenuation, _additionalLightAttenuations);
                cmd.SetGlobalVectorArray(PropertyID.AdditionalLightsSpotDir, _additionalLightSpotDirections);
                cmd.SetGlobalVectorArray(PropertyID.AdditionalLightOcclusionProbeChannel, _additionalLightOcclusionProbeChannels);
            }
            cmd.SetGlobalVector(PropertyID.WorldSpaceCameraPos, _worldSpaceCameraPos);
            SetViewAndProjectionMatrices(cmd);
        }

        // --- RasterCommandBuffer용 메서드 ---
        // PortalPassNode 클래스 내부의 SetViewAndProjectionMatrices 메서드를 아래 내용으로 교체하세요.

        public void SetViewAndProjectionMatrices(RasterCommandBuffer cmd, bool isDepthNormals = false)
        {
            if (renderNode == null) return;

            Matrix4x4 viewMatrix = renderNode.worldToCameraMatrix;
            Matrix4x4 projectionMatrix = renderNode.projectionMatrix;

            Debug.Log($"<color=yellow>[Portal Matrix]</color> Depth: {renderNode.depth}, CamPos: {renderNode.worldToCameraMatrix.inverse.GetColumn(3)}");

            // 유니티 6 표준 행렬 설정
            cmd.SetViewProjectionMatrices(viewMatrix, projectionMatrix);

            // [중요] 쉐이더 내부 변수 강제 동기화
            cmd.SetGlobalMatrix("_MatrixV", viewMatrix);
            cmd.SetGlobalMatrix("_MatrixVP", projectionMatrix * viewMatrix);

            // 월드 카메라 좌표 갱신
            Vector3 portalCameraPos = renderNode.localToWorldMatrix.GetColumn(3);
            cmd.SetGlobalVector("_WorldSpaceCameraPos", portalCameraPos);
        }

        public void RestoreState(RasterCommandBuffer cmd)
        {
            cmd.SetGlobalVector(PropertyID.MainLightPosition, _lightPos);
            cmd.SetGlobalVector(PropertyID.MainLightColor, _lightColor);
            cmd.SetGlobalVector(PropertyID.MainLightOcclusionProbesChannel, _lightOcclusionChannel);
            cmd.SetGlobalVector(PropertyID.AdditionalLightsCount, _additionalLightsCount);
            if (_additionalLightsCount.x != 0f)
            {
                cmd.SetGlobalVectorArray(PropertyID.AdditionalLightsPosition, _additionalLightPositions);
                cmd.SetGlobalVectorArray(PropertyID.AdditionalLightsColor, _additionalLightColors);
                cmd.SetGlobalVectorArray(PropertyID.AdditionalLightsAttenuation, _additionalLightAttenuations);
                cmd.SetGlobalVectorArray(PropertyID.AdditionalLightsSpotDir, _additionalLightSpotDirections);
                cmd.SetGlobalVectorArray(PropertyID.AdditionalLightOcclusionProbeChannel, _additionalLightOcclusionProbeChannels);
            }
            cmd.SetGlobalVector(PropertyID.WorldSpaceCameraPos, _worldSpaceCameraPos);
            SetViewAndProjectionMatrices(cmd);
        }

        public void StoreState(ref RenderingData renderingData)
        {
            _renderingData = renderingData;
            _lightPos = Shader.GetGlobalVector(PropertyID.MainLightPosition);
            _lightColor = Shader.GetGlobalVector(PropertyID.MainLightColor);
            _lightOcclusionChannel = Shader.GetGlobalVector(PropertyID.MainLightOcclusionProbesChannel);
            _additionalLightsCount = Shader.GetGlobalVector(PropertyID.AdditionalLightsCount);
            if (_additionalLightsCount.x != 0f)
            {
                GetGlobalVectorArray(PropertyID.AdditionalLightsPosition, ref _additionalLightPositions);
                GetGlobalVectorArray(PropertyID.AdditionalLightsColor, ref _additionalLightColors);
                GetGlobalVectorArray(PropertyID.AdditionalLightsAttenuation, ref _additionalLightAttenuations);
                GetGlobalVectorArray(PropertyID.AdditionalLightsSpotDir, ref _additionalLightSpotDirections);
                GetGlobalVectorArray(PropertyID.AdditionalLightOcclusionProbeChannel, ref _additionalLightOcclusionProbeChannels);
            }
            _worldSpaceCameraPos = Shader.GetGlobalVector(PropertyID.WorldSpaceCameraPos);
        }

        private void GetGlobalVectorArray(int id, ref Vector4[] array)
        {
            Shader.GetGlobalVectorArray(id, tempList);
            if (array == null || array.Length != tempList.Count) array = new Vector4[tempList.Count];
            for (int i = 0; i < array.Length; i++) array[i] = tempList[i];
            tempList.Clear();
        }
    }

    internal static class PortalPassGroupPool
    {
        private static List<PortalPassNode> _groups = new List<PortalPassNode>();
        internal static PortalPassNode Get()
        {
            if (_groups.Count > 0)
            {
                PortalPassNode group = _groups[_groups.Count - 1];
                _groups.RemoveAt(_groups.Count - 1);
                group.renderNode = null;
                group.colorTexture = null;
                return group;
            }
            return new PortalPassNode();
        }
        internal static void Release(PortalPassNode node) { if (node != null) _groups.Add(node); }
    }
}