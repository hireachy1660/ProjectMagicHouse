using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using VRPortalToolkit.Data;

namespace VRPortalToolkit.Rendering
{
    [RequireComponent(typeof(MeshFilter)), ExecuteInEditMode]
    public class PortalMeshRenderer : PortalRendererBase
    {
        [SerializeField] private Portal _portal;
        public Portal portal { get => _portal; set => _portal = value; }
        public override IPortal Portal => _portal;

        [SerializeField] private PortalMeshRenderer _connectedRenderer;
        public PortalMeshRenderer connectedRenderer { get => _connectedRenderer; set => _connectedRenderer = value; }

        private MeshFilter _filter;
        public MeshFilter filter => _filter ? _filter : _filter = GetComponent<MeshFilter>();

        [SerializeField] private Transform _clippingPlane;
        public Transform clippingPlane { get => _clippingPlane; set => _clippingPlane = value; }

        [SerializeField] private ClippingMode _clippingMode;
        public ClippingMode clippingMode { get => _clippingMode; set => _clippingMode = value; }

        public enum ClippingMode { None = 0, OneSided = 1, DoubleSided = 2 }

        [SerializeField] private CullMode _cullMode = CullMode.Back;
        public CullMode cullMode { get => _cullMode; set => _cullMode = value; }

        [SerializeField] private float _clippingOffset = 0.001f;
        public float clippingOffset { get => _clippingOffset; set => _clippingOffset = value; }

        [SerializeField] private Material _defaultMaterial;
        public Material defaultMaterial { get => _defaultMaterial; set => _defaultMaterial = value; }

        [SerializeField] private PortalRendererSettings _overrides;
        public PortalRendererSettings overrides { get => _overrides; set => _overrides = value; }
        public override PortalRendererSettings Overrides => _overrides;

        public UnityAction<PortalRenderNode> preCull;
        public UnityAction<PortalRenderNode> postCull;
        public UnityAction<PortalRenderNode> postRender;

        protected virtual void Reset()
        {
            _portal = GetComponentInChildren<Portal>(true);
            if (!_portal) _portal = GetComponentInParent<Portal>();
            _clippingPlane = transform;
        }

        protected virtual void OnDrawGizmos()
        {
            // 1. 기존 메쉬 가이드 표시
            if (filter && _filter.sharedMesh)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.color = (portal && portal.connected) ? Color.clear : new Color(0.5f, 0.5f, 0.5f, 0.2f);
                Gizmos.DrawMesh(_filter.sharedMesh);
            }

            // 2. [가상 카메라 시각화] 씬 뷰에서 보라색 구체로 카메라 위치 표시
            if (portal != null && portal.connected != null)
            {
                Camera mainCam = Camera.main;
                if (mainCam != null)
                {
                    // 상대 행렬 계산: 내 포탈 -> 상대 포탈로의 시점 변환
                    Matrix4x4 m = portal.connected.transform.localToWorldMatrix * portal.transform.worldToLocalMatrix * mainCam.transform.localToWorldMatrix;
                    Vector3 virtualPos = m.GetColumn(3);

                    Gizmos.matrix = Matrix4x4.identity;
                    Gizmos.color = Color.magenta; // 보라색
                    Gizmos.DrawWireSphere(virtualPos, 0.3f); // 가상 카메라 위치
                    Gizmos.DrawLine(virtualPos, portal.connected.transform.position); // 연결선

                    // 시선 방향 표시
                    Gizmos.DrawRay(virtualPos, m.rotation * Vector3.forward * 1.0f);
                }
            }
        }

        public override bool TryGetWindow(PortalRenderNode renderNode, Vector3 cameraPosition, Matrix4x4 view, Matrix4x4 proj, out ViewWindow innerWindow)
        {
            if (!isActiveAndEnabled || !filter || !_filter.sharedMesh)
            {
                innerWindow = default;
                return false;
            }

            if (_clippingPlane && _clippingMode == ClippingMode.OneSided && !IsOnFrontSide(cameraPosition))
            {
                innerWindow = default;
                return false;
            }

            innerWindow = ViewWindow.GetWindow(view, proj, _filter.sharedMesh.bounds, transform.localToWorldMatrix);
            return true;
        }

        private bool IsOnFrontSide(Vector3 position)
        {
            return Vector3.Dot(position - _clippingPlane.position, _clippingPlane.forward) > 0f;
        }

        public override void PreCull(PortalRenderNode renderNode) => preCull?.Invoke(renderNode);
        public override void PostCull(PortalRenderNode renderNode) => postCull?.Invoke(renderNode);

        // PortalMeshRenderer.cs 내부 수정
        public override void Render(PortalRenderNode renderNode, RasterCommandBuffer commandBuffer, Material material, MaterialPropertyBlock properties = null)
        {
            if (material == null) material = _defaultMaterial;
            if (material == null) return;

            // 로그가 찍히는 걸 확인했으니 이 부분은 정상 작동 중입니다.
            //Debug.Log($"[Portal Render] {gameObject.name} 그리는 중... 머티리얼: {material.name}");

            if (isActiveAndEnabled && filter && filter.sharedMesh)
            {
                Matrix4x4 localToWorld = transform.localToWorldMatrix;
                Mesh mesh = filter.sharedMesh;

                for (int i = 0; i < mesh.subMeshCount; i++)
                {
                    // shaderPass를 0으로 명시하고, submesh 인덱스 i를 정확히 전달
                    commandBuffer.DrawMesh(mesh, localToWorld, material, i, 0, properties);
                }
            }
        }

        public override void RenderDefault(PortalRenderNode renderNode, RasterCommandBuffer commandBuffer)
        {
            if (isActiveAndEnabled && defaultMaterial)
                Render(renderNode, commandBuffer, defaultMaterial);
        }

        public override void PostRender(PortalRenderNode renderNode) => postRender?.Invoke(renderNode);

        public override bool TryGetClippingPlane(PortalRenderNode renderNode, out Vector3 clippingPlaneCentre, out Vector3 clippingPlaneNormal)
        {
            if (_clippingPlane)
            {
                if (_clippingMode == ClippingMode.DoubleSided && !IsOnFrontSide(renderNode.localToWorldMatrix.GetColumn(3)))
                    clippingPlaneNormal = -_clippingPlane.forward;
                else
                    clippingPlaneNormal = _clippingPlane.forward;

                clippingPlaneCentre = _clippingPlane.transform.position + clippingPlaneNormal * _clippingOffset;
                return true;
            }
            clippingPlaneCentre = clippingPlaneNormal = default;
            return false;
        }
    }
}