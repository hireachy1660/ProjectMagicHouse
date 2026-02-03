using Misc.EditorHelpers;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using VRPortalToolkit.Data;
using VRPortalToolkit.Rendering;

namespace VRPortalToolkit.XRI
{
    public class XRPortalOverlay : PortalRendererBase
    {
        private static Mesh _circleMesh;
        private static Mesh _squareMesh;

        [SerializeField] private Transform _origin;
        public Transform origin { get => _origin; set => _origin = value; }

        public enum Transition { None = 0, Circle = 1, Square = 2 }
        [SerializeField] private Transition _transition;

#if UNITY_EDITOR
        private bool isAnimated => _transition != Transition.None;
        [ShowIf(nameof(isAnimated))]
#endif
        [SerializeField] private float _transitionTime = 1f;
        [SerializeField] private bool _requireSelected = true;
        [SerializeField] private Trigger _triggers = Trigger.DirectionInUse;

        [Flags]
        public enum Trigger { None = 0, IsActivated = 1 << 1, DirectionInUse = 1 << 2, VelocityThreshold = 1 << 3, ManualTrigger = 1 << 4 }

        [Serializable]
        private struct RaycastClipping
        {
            public LayerMask raycastMask;
            public QueryTriggerInteraction raycastTriggerInteraction;
            public float raycastRadius;
            public float clippingOffset;
        }

        [SerializeField] private RaycastClipping _raycastClipping = new RaycastClipping() { clippingOffset = 0.1f };
        [SerializeField] private PortalRendererSettings _overrides;

        public override PortalRendererSettings Overrides => _overrides;
        public override IPortal Portal => _interactable != null ? _interactable.portal : null;

        private XRPortalInteractable _interactable;
        private IXRSelectInteractor _interactor;
        private XRBaseController _interactorController;
        private float _transitionState;
        private float _triggeredTimer;
        private bool _triggered;
        private bool _isActivating;

        protected virtual void Awake() => _interactable = GetComponent<XRPortalInteractable>();

        protected override void OnEnable()
        {
            base.OnEnable();
            if (_interactable)
            {
                _interactable.activated.AddListener(OnActivated);
                _interactable.deactivated.AddListener(OnDeactivated);
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (_interactable)
            {
                _interactable.activated.RemoveListener(OnActivated);
                _interactable.deactivated.RemoveListener(OnDeactivated);
            }
        }

        private void OnActivated(ActivateEventArgs args) => _isActivating = true;
        private void OnDeactivated(DeactivateEventArgs args) => _isActivating = false;

        private void LateUpdate()
        {
            if (_interactable == null) return;

            if (_interactor == null && _interactable.isSelected && _interactable.interactorsSelecting.Count > 0)
            {
                _interactor = _interactable.interactorsSelecting[0];
                _interactorController = _interactor.transform.gameObject.GetComponentInParent<XRBaseController>();
            }
            else if (!_interactable.isSelected)
            {
                _interactor = null;
                _interactorController = null;
            }

            if (_triggered)
            {
                _triggeredTimer += Time.deltaTime;
                if (_triggeredTimer > 3f) _triggered = false; // 기본 3초
            }

            if (HasTrigger()) { _triggeredTimer = 0f; _triggered = true; }
            if (_requireSelected && !_interactable.isSelected) _triggered = false;

            float step = _transitionTime <= 0 ? 1f : (Time.deltaTime / _transitionTime);
            if (_triggered) _transitionState = Mathf.Min(1f, _transitionState + step);
            else _transitionState = Mathf.Max(0f, _transitionState - step);
        }

        private bool HasTrigger()
        {
            if (_isActivating) return true;
            if (_interactorController is ActionBasedController controller)
            {
                var action = controller.directionalAnchorRotationAction.action;
                if (action != null && action.ReadValue<Vector2>().sqrMagnitude > 0.1f) return true;
            }
            return false;
        }

        public override bool TryGetWindow(PortalRenderNode renderNode, Vector3 cameraPosition, Matrix4x4 view, Matrix4x4 proj, out ViewWindow innerWindow)
        {
            if (isActiveAndEnabled && _transitionState > 0f && renderNode.depth == 0)
            {
                innerWindow = new ViewWindow(1f, 1f, 0f);
                return true;
            }
            innerWindow = default;
            return false;
        }

        // --- 에러 해결: 정확한 메서드 선언 ---
        public override void RenderDefault(PortalRenderNode renderNode, RasterCommandBuffer commandBuffer)
        {
            // 아무것도 렌더링하지 않음
        }

        public override void Render(PortalRenderNode renderNode, RasterCommandBuffer commandBuffer, Material material, MaterialPropertyBlock properties = null)
        {
            if (isActiveAndEnabled && _transitionState > 0f)
            {
                commandBuffer.DrawMesh(_transition == Transition.Circle ? GetCircleMesh() : GetSquareMesh(),
                    GetTransitionLocalToWorld(renderNode.camera), material, 0, 0, properties);
            }
        }

        private Matrix4x4 GetTransitionLocalToWorld(Camera camera)
        {
            Vector3 pos = _origin ? _origin.position : transform.position;
            return Matrix4x4.TRS(pos, camera.transform.rotation, Vector3.one * _transitionState);
        }

        public override bool TryGetClippingPlane(PortalRenderNode renderNode, out Vector3 clippingPlaneCentre, out Vector3 clippingPlaneNormal)
        {
            clippingPlaneCentre = clippingPlaneNormal = default;
            return false;
        }

        private static Mesh GetCircleMesh() { if (!_circleMesh) _circleMesh = new Mesh(); return _circleMesh; }
        private static Mesh GetSquareMesh() { if (!_squareMesh) _squareMesh = new Mesh(); return _squareMesh; }
    }
}