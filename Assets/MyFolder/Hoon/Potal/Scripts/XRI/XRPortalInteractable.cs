using EzySlice;
using Misc.EditorHelpers;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using VRPortalToolkit.Physics;
using static UnityEngine.XR.Interaction.Toolkit.XRInteractionUpdateOrder;

namespace VRPortalToolkit.XRI
{
    /// <summary>
    /// XR interactable for portals, supporting grab, snap, and linked movement.
    /// Updated for Unity 6 (6000.3.x) and XRI 3.0
    /// </summary>
    public class XRPortalInteractable : UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable
    {
        private readonly WaitForEndOfFrame _WaitForEndOfFrame = new WaitForEndOfFrame();

        const float k_DeltaTimeThreshold = 0.001f;

        [Tooltip("The portal associated with this interactable.")]
        [SerializeField] private Portal _portal;
        public Portal portal { get => _portal; set => _portal = value; }

        [Tooltip("The connected portal's XRPortalInteractable.")]
        [SerializeField] private XRPortalInteractable _connected;
        public XRPortalInteractable connected { get => _connected; set => _connected = value; }

        [Tooltip("The transform representing the ground level.")]
        [SerializeField] private Transform _groundLevel;
        public Transform groundLevel { get => _groundLevel; set => _groundLevel = value; }

        [Tooltip("How the opposite portal moves when this portal is moved")]
        [SerializeField] private LinkedMovement _linkedMovement = LinkedMovement.Anchored;
        public LinkedMovement linkedMovement
        {
            get => _linkedMovement;
            set
            {
                if (_linkedMovement != value)
                {
                    _linkedMovement = value;
                    StoreAnchor();
                }
            }
        }

        public enum LinkedMovement { None = 0, Relative = 1, Anchored = 2 }

        [Tooltip("How the portal's levelness is handled.")]
        [SerializeField] private Levelness _levelness = Levelness.LevelElevation | Levelness.LevelOrientation;
        public Levelness levelness { get => _levelness; set => _levelness = value; }

        [Flags]
        public enum Levelness { None = 0, LevelElevation = 1 << 1, LevelOrientation = 1 << 2 }

        [SerializeField] private bool _useSnapDistanceThreshold = true;
        public bool useSnapDistanceThreshold { get => _useSnapDistanceThreshold; set => _useSnapDistanceThreshold = value; }

        [ShowIf(nameof(_useSnapDistanceThreshold))]
        [SerializeField] private float _snapDistanceThreshold = 0.1f;
        public float snapDistanceThreshold { get => _snapDistanceThreshold; set => _snapDistanceThreshold = value; }

        [SerializeField] private bool _useSnapAngleThreshold = true;
        public bool useSnapAngleThreshold { get => _useSnapAngleThreshold; set => _useSnapAngleThreshold = value; }

        [ShowIf(nameof(_useSnapAngleThreshold))]
        [SerializeField] private float _snapAngleThreshold = 3f;
        public float snapAngleThreshold { get => _snapAngleThreshold; set => _snapAngleThreshold = value; }

        private PortalRelativePosition _interactorPositioning;
        private Transform _interactorOrigin;
        private Vector3 _forward;
        private Rigidbody _rigidbody;

        private Matrix4x4 _connectedAnchor;
        private Matrix4x4 _anchor;

        private float _lastGrabTime;

        protected override void Awake()
        {
            base.Awake();
            _rigidbody = GetComponent<Rigidbody>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            StoreAnchor();
            PortalPhysics.AddPostTeleportListener(transform, OnPostTeleport);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            PortalPhysics.RemovePostTeleportListener(transform, OnPostTeleport);
        }

        public override bool IsSelectableBy(UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor interactor)
        {
            if (base.IsSelectableBy(interactor))
            {
                if (IsSelected(interactor)) return true;
                if (IsWithinSnapThreshold() && _connected && (_connected.isSelected || _connected._lastGrabTime > _lastGrabTime))
                    return false;

                return true;
            }
            return false;
        }

        protected override void OnSelectEntering(SelectEnterEventArgs args)
        {
            PortalPhysics.UnregisterPortable(transform);
            Transform interactor = args.interactorObject.transform;
            _interactorPositioning = interactor.GetComponentInParent<PortalRelativePosition>();

            if (_connected && _connected.isSelected && interactorsSelecting.Count == 1)
                _connected.SetupRigidbodyDrop(_connected._rigidbody);

            if (_interactorPositioning)
            {
                Pose interactorPose = new Pose(interactor.position, interactor.rotation);
                _interactorPositioning.GetPortalsToSource().ModifyTransform(interactor);
                base.OnSelectEntering(args);
                interactor.transform.SetPositionAndRotation(interactorPose.position, interactorPose.rotation);

                _interactorOrigin = _interactorPositioning.origin;
                AddOriginListener();
            }
            else
                base.OnSelectEntering(args);

            if (_connected && !_connected.isSelected)
            {
                if (_connected._rigidbody) _connected.SetupRigidbodyGrab(_connected._rigidbody);
                if (IsWithinSnapThreshold())
                    _connected.transform.SetPositionAndRotation(transform.position, Quaternion.LookRotation(-transform.forward, transform.up));

                _connected.StoreAnchor();
            }
            _lastGrabTime = Time.time;
        }

        public virtual bool IsWithinSnapThreshold()
        {
            if (!_connected) return false;
            bool distMatch = !_useSnapDistanceThreshold || Vector3.Distance(transform.position, connected.transform.position) < _snapDistanceThreshold;
            bool angleMatch = !_useSnapAngleThreshold || Quaternion.Angle(transform.rotation, Quaternion.LookRotation(-connected.transform.forward, connected.transform.up)) < _snapAngleThreshold;
            return distMatch && angleMatch;
        }

        protected override void OnSelectExiting(SelectExitEventArgs args)
        {
            base.OnSelectExiting(args);
            if (_connected && _connected._rigidbody && !_connected.isSelected)
                _connected.SetupRigidbodyDrop(_connected._rigidbody);

            if (_connected && _connected.isSelected)
            {
                SetupRigidbodyGrab(_rigidbody);
                StoreAnchor();
            }

            RemoveOriginListener();
            _interactorPositioning = null;
            _lastGrabTime = Time.time;
        }

        public override void ProcessInteractable(UpdatePhase updatePhase)
        {
            if (_interactorOrigin != (_interactorPositioning ? _interactorPositioning.origin : null))
            {
                RemoveOriginListener();
                _interactorOrigin = _interactorPositioning ? _interactorPositioning.origin : null;
                AddOriginListener();
            }

            if (isSelected)
            {
                Matrix4x4 previous = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
                if (_interactorPositioning)
                {
                    Transform interactor = interactorsSelecting[0].transform;
                    Pose interactorPose = new Pose(interactor.position, interactor.rotation);
                    _interactorPositioning.GetPortalsToSource().ModifyTransform(interactor);
                    base.ProcessInteractable(updatePhase);
                    interactor.transform.SetPositionAndRotation(interactorPose.position, interactorPose.rotation);
                }
                else
                    base.ProcessInteractable(updatePhase);

                Pose targetPose = XRUtils.GetTargetPose(this);
                _connected?.ProcessLinkedMovement(updatePhase, previous, Matrix4x4.TRS(targetPose.position, targetPose.rotation, transform.localScale));
            }
            else
                base.ProcessInteractable(updatePhase);
        }

        protected virtual void ProcessLinkedMovement(UpdatePhase updatePhase, in Matrix4x4 connectedPrevious, in Matrix4x4 connectedTarget)
        {
            if (!isSelected)
            {
                GetLinkedTargetPose(connectedPrevious, connectedTarget, out Pose targetPose, out Vector3 targetScale);
                switch (updatePhase)
                {
                    case UpdatePhase.Fixed:
                        if (movementType == MovementType.VelocityTracking)
                            PerformVelocityTrackingUpdate(Time.deltaTime, targetPose);
                        else if (movementType == MovementType.Kinematic)
                            PerformKinematicUpdate(targetPose);
                        transform.localScale = targetScale;
                        break;
                    case UpdatePhase.Dynamic:
                    case UpdatePhase.OnBeforeRender:
                        if (movementType == MovementType.Instantaneous)
                        {
                            transform.SetPositionAndRotation(targetPose.position, targetPose.rotation);
                            transform.localScale = targetScale;
                        }
                        break;
                }
            }
        }

        private void GetLinkedTargetPose(Matrix4x4 connectedPrevious, in Matrix4x4 connectedTarget, out Pose pose, out Vector3 scale)
        {
            if (_linkedMovement != LinkedMovement.None)
            {
                if (_linkedMovement == LinkedMovement.Relative)
                {
                    _anchor = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
                    _connectedAnchor = Matrix4x4.TRS(connectedPrevious.GetColumn(3),
                        Quaternion.LookRotation(connectedPrevious.MultiplyVector(Vector3.back), connectedPrevious.MultiplyVector(Vector3.up)),
                        connectedPrevious.lossyScale);
                }

                Matrix4x4 target = (_anchor * _connectedAnchor.inverse) * connectedTarget;
                pose = new Pose(target.GetColumn(3), Quaternion.LookRotation(target.MultiplyVector(Vector3.back), target.MultiplyVector(Vector3.up)));
                scale = target.lossyScale;
            }
            else
            {
                pose = new Pose(transform.position, transform.rotation);
                scale = transform.localScale;
            }

            Pose connectedTargetPose = new Pose(connectedTarget.GetColumn(3), connectedTarget.rotation);
            if (_levelness.HasFlag(Levelness.LevelOrientation)) LevelOrientation(connectedTargetPose, ref pose);
            if (_levelness.HasFlag(Levelness.LevelElevation)) LevelElevation(connectedTargetPose, ref pose);
        }

        private void StoreAnchor()
        {
            _anchor = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
            if (_connected)
                _connectedAnchor = Matrix4x4.TRS(_connected.transform.position,
                    Quaternion.LookRotation(-_connected.transform.forward, _connected.transform.up),
                    _connected.transform.localScale);
        }

        protected virtual void LevelOrientation(Pose connectedTargetPose, ref Pose targetPose)
        {
            _forward = GetForward(targetPose.forward);
            Quaternion connectedRot = _connected && _connected._groundLevel
                ? Quaternion.Inverse(_connected._groundLevel.transform.rotation) * connectedTargetPose.rotation
                : connectedTargetPose.rotation;

            targetPose.rotation = _groundLevel ? _groundLevel.rotation * connectedRot : connectedRot;
            targetPose.rotation = Quaternion.LookRotation(-targetPose.forward, targetPose.up);

            Vector3 newForward = GetForward(targetPose.forward), up = GetUp();
            targetPose.rotation = Quaternion.AngleAxis(Vector3.SignedAngle(newForward, _forward, up), up) * targetPose.rotation;
        }

        protected virtual void LevelElevation(Pose connectedTargetPose, ref Pose targetPose)
        {
            if (_connected)
            {
                Vector3 connectedPos = _connected && _connected._groundLevel
                    ? _connected._groundLevel.InverseTransformPoint(connectedTargetPose.position)
                    : connectedTargetPose.position;

                if (_groundLevel)
                {
                    Vector3 originalPos = _groundLevel.InverseTransformPoint(targetPose.position);
                    targetPose.position = _groundLevel.TransformPoint(new Vector3(originalPos.x, connectedPos.y, originalPos.z));
                }
                else
                    targetPose.position = new Vector3(targetPose.position.x, connectedPos.y, targetPose.position.z);
            }
        }

        private Vector3 GetUp() => _groundLevel ? _groundLevel.up : Vector3.up;
        private Vector3 GetForward(Vector3 forward)
        {
            forward = Vector3.ProjectOnPlane(forward, GetUp());
            return forward.magnitude == 0 ? _forward : forward;
        }

        private void PerformKinematicUpdate(Pose targetPose)
        {
            // Unity 6에서는 attachPointCompatibilityMode가 삭제되었으므로 항상 Default 로직 적용
            _rigidbody.MovePosition(targetPose.position);
            _rigidbody.MoveRotation(targetPose.rotation);
        }

        private void PerformVelocityTrackingUpdate(float deltaTime, Pose targetPose)
        {
            if (deltaTime < k_DeltaTimeThreshold) return;

            // Unity 6: velocity -> linearVelocity
            _rigidbody.linearVelocity *= (1f - velocityDamping);
            var positionDelta = targetPose.position - transform.position;
            var velocity = positionDelta / deltaTime;
            _rigidbody.linearVelocity += (velocity * velocityScale);

            _rigidbody.angularVelocity *= (1f - angularVelocityDamping);
            var rotationDelta = targetPose.rotation * Quaternion.Inverse(transform.rotation);
            rotationDelta.ToAngleAxis(out var angleInDegrees, out var rotationAxis);
            if (angleInDegrees > 180f) angleInDegrees -= 360f;

            if (Mathf.Abs(angleInDegrees) > Mathf.Epsilon)
            {
                var angularVelocity = (rotationAxis * (angleInDegrees * Mathf.Deg2Rad)) / deltaTime;
                _rigidbody.angularVelocity += (angularVelocity * angularVelocityScale);
            }
        }

        protected virtual void OnOriginPreTeleport(Teleportation teleportation)
        {
            _originPreTeleportLocalPose = teleportation.transform.InverseTransformPose(new Pose(transform.position, transform.rotation));
        }

        protected virtual void OnOriginPostTeleport(Teleportation teleportation)
        {
            if (isSelected)
            {
                UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor interactor = interactorsSelecting[0];
                interactionManager.SelectExit(interactor, this);

                if (teleportation.fromPortal == portal)
                {
                    if (_connected) interactionManager.SelectEnter(interactor, _connected);
                }
                else
                {
                    Pose newPose = teleportation.transform.TransformPose(_originPreTeleportLocalPose);
                    transform.SetPositionAndRotation(newPose.position, newPose.rotation);
                    interactionManager.SelectEnter(interactor, this);
                }
            }
        }

        private Pose _originPreTeleportLocalPose;

        private void AddOriginListener()
        {
            if (_interactorOrigin != null)
            {
                PortalPhysics.AddPreTeleportListener(_interactorOrigin, OnOriginPreTeleport);
                PortalPhysics.AddPostTeleportListener(_interactorOrigin, OnOriginPostTeleport);
            }
        }

        private void RemoveOriginListener()
        {
            if (_interactorOrigin != null)
            {
                PortalPhysics.RemovePreTeleportListener(_interactorOrigin, OnOriginPreTeleport);
                PortalPhysics.RemovePostTeleportListener(_interactorOrigin, OnOriginPostTeleport);
            }
        }

        private void OnPostTeleport(Teleportation teleportation) => StoreAnchor();
    }
}