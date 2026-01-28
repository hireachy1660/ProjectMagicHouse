using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors; // 추가
using VRPortalToolkit.Cloning;
using VRPortalToolkit.Physics;

namespace VRPortalToolkit.XRI
{
    /// <summary>
    /// Portal-aware ray interactor that supports raycast interactions through portals.
    /// Updated for Unity 6 (6000.3.x) and XRI 3.0
    /// </summary>
    public class XRPortableRayInteractor : UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor, IXRPortableInteractor, IPortalLineRenderable, IPortalCursorRenderable
    {
        private readonly static int MaxPortals = 10;
        private readonly static List<UnityEngine.XR.Interaction.Toolkit.Interactables.IXRInteractable> _results = new List<UnityEngine.XR.Interaction.Toolkit.Interactables.IXRInteractable>(1);
        private readonly static PortalRay[] castPortalRays = new PortalRay[MaxPortals];

        [Tooltip("The layer mask used for portal raycasting.")]
        [SerializeField] private LayerMask _portalMask = 1 << 3;
        public virtual LayerMask portalMask { get => _portalMask; set => _portalMask = value; }

        [Tooltip("The trigger interaction mode for portal raycasting.")]
        [SerializeField] private QueryTriggerInteraction _portalTriggerInteraction;
        public virtual QueryTriggerInteraction portalTriggerInteraction { get => _portalTriggerInteraction; set => _portalTriggerInteraction = value; }

        public int portalRayCount => _portalRaysCount;

        private UnityEngine.XR.Interaction.Toolkit.Interactables.IXRInteractable _actualValidTarget;
        private Vector3[] linePoints;
        private PortalRay[] _portalRays;
        private int _portalRaysCount;
        private int _portalIndex;
        private RaycastHit _hitInfo;

        public IEnumerable<Portal> GetPortalsToInteractable(UnityEngine.XR.Interaction.Toolkit.Interactables.IXRInteractable interactable)
        {
            IEnumerable<Portal> from = GetPortalsToRaycastHit(), to = null;

            if (hasSelection && interactablesSelected[0] == interactable && _hitInfo.collider)
            {
                if (PortalCloning.TryGetCloneInfo(_hitInfo.collider.transform, out var info))
                    to = info.GetCloneToOriginalPortals();
            }

            return from.Difference(to);
        }

        private IEnumerable<Portal> GetPortalsToRaycastHit()
        {
            for (int i = 1; i <= _portalIndex; i++)
                yield return _portalRays[i].fromPortal;
        }

        protected override void OnSelectEntering(SelectEnterEventArgs args)
        {
            base.OnSelectEntering(args);

            if (!useForceGrab && interactablesSelected.Count == 1 && _portalIndex != -1)
            {
                Vector3 point = _hitInfo.point;

                for (int i = _portalIndex; i > 0; i--)
                    _portalRays[i].fromPortal?.connected.ModifyPoint(ref point);

                attachTransform.position = point;
            }
        }

        public override void PreprocessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            LayerMask temp = raycastMask;
            raycastMask = 0;
            base.PreprocessInteractor(updatePhase);
            raycastMask = temp;

            if (updatePhase != XRInteractionUpdateOrder.UpdatePhase.Dynamic)
                return;

            GetLinePoints(ref linePoints, out int numPoints);

            if (_portalRays == null || _portalRays.Length - MaxPortals < numPoints)
                _portalRays = new PortalRay[numPoints + MaxPortals];

            _portalRaysCount = 0;
            if (!hasSelection) _actualValidTarget = null;

            if (linePoints.Length > 1)
            {
                Matrix4x4 teleportMatrix = Matrix4x4.identity;
                Vector3 from = linePoints[0], to;
                for (int i = 1; i < numPoints && _portalRaysCount < _portalRays.Length; i++)
                {
                    to = teleportMatrix.MultiplyPoint3x4(linePoints[i]);
                    Vector3 direction = (to - from).normalized;
                    float maxDistance = Vector3.Distance(to, from);

                    int castRayCount = PortalPhysics.GetRays(from, direction, castPortalRays, maxDistance, portalMask, portalTriggerInteraction);

                    for (int j = 0; j < castRayCount && _portalRaysCount < _portalRays.Length; j++)
                    {
                        _portalRays[_portalRaysCount++] = castPortalRays[j];
                        Portal portal = castPortalRays[j].fromPortal;
                        if (portal)
                        {
                            teleportMatrix = castPortalRays[j].fromPortal.teleportMatrix * teleportMatrix;
                            castPortalRays[j].fromPortal.ModifyPoint(ref to);
                        }
                    }
                    from = to;
                }

                IPhysicsCaster caster = (hitDetectionType == HitDetectionType.SphereCast && sphereCastRadius > 0f)
                    ? new SphereCaster(sphereCastRadius) : new Raycaster();

                if (PortalPhysics.Cast(caster, _portalRays, _portalRaysCount, out _hitInfo, out _portalIndex, raycastMask, raycastTriggerInteraction))
                {
                    Collider collider = PortalCloning.GetOriginal(_hitInfo.collider);
                    if (interactionManager.TryGetInteractableForCollider(collider, out _actualValidTarget, out UnityEngine.XR.Interaction.Toolkit.Interactables.XRInteractableSnapVolume snapVolume))
                    {
                        bool baseQueryHitsTriggers = raycastTriggerInteraction == QueryTriggerInteraction.Collide ||
                            (raycastTriggerInteraction == QueryTriggerInteraction.UseGlobal && UnityEngine.Physics.queriesHitTriggers);

                        if (raycastSnapVolumeInteraction == QuerySnapVolumeInteraction.Ignore && baseQueryHitsTriggers)
                        {
                            if (snapVolume == null) _actualValidTarget = null;
                        }
                        else if (raycastSnapVolumeInteraction == QuerySnapVolumeInteraction.Collide && !baseQueryHitsTriggers)
                        {
                            if (snapVolume != null) _actualValidTarget = null;
                        }
                    }
                }
            }
            else _portalIndex = -1;

            if (_portalIndex == -1)
                XRUtils.SetRaycastHitsCount(this, -1);
            else
            {
                XRUtils.SetRaycastHitsCount(this, 1);
                XRUtils.GetRaycastHits(this)[0] = _hitInfo;
            }
        }

        public override void GetValidTargets(List<UnityEngine.XR.Interaction.Toolkit.Interactables.IXRInteractable> targets)
        {
            targets.Clear();
            if (_actualValidTarget != null) targets.Add(_actualValidTarget);

            var filter = targetFilter;
            if (filter != null && filter.canProcess)
            {
                filter.Process(this, targets, _results);
                targets.Clear();
                targets.AddRange(_results);
            }
        }

        public new bool TryGetHitInfo(out Vector3 position, out Vector3 normal, out int portalRayIndex, out bool isValidTarget)
        {
            position = _hitInfo.point;
            normal = _hitInfo.normal;
            portalRayIndex = _portalIndex;
            isValidTarget = hasSelection && _actualValidTarget != null;
            return _portalIndex >= 0;
        }

        public PortalRay GetPortalRay(int portalRayIndex) => _portalRays[portalRayIndex];

        public bool TryGetCursor(out Pose cursorPose, out bool isValidTarget)
        {
            if (TryGetHitInfo(out Vector3 position, out Vector3 normal, out _, out isValidTarget))
            {
                cursorPose.position = position;
                Vector3 up = transform.up;
                for (int i = 1; i < _portalIndex; i++)
                    _portalRays[i].fromPortal?.ModifyDirection(up);

                cursorPose.rotation = attachTransform.rotation;

                // 유니티 6/XRI 3.0 방식: interactablesHovered 리스트에서 첫 번째 요소를 가져옴
                var oldestHovered = interactablesHovered.Count > 0 ? interactablesHovered[0] : null;

                if (oldestHovered is UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals.IXRReticleDirectionProvider reticleDirectionProvider)
                {
                    reticleDirectionProvider.GetReticleDirection(this, normal, out var reticleUp, out var reticleForward);

                    if (reticleForward.HasValue)
                        cursorPose.rotation = Quaternion.LookRotation(reticleForward.Value, reticleUp);
                    else
                        cursorPose.rotation = Quaternion.LookRotation(Vector3.Slerp(reticleUp, -reticleUp, 0.5f), reticleUp);
                }
                else
                    cursorPose.rotation = Quaternion.LookRotation(Vector3.Slerp(normal, -normal, 0.5f), normal);

                return true;
            }

            cursorPose = default;
            return false;
        }
    }
}