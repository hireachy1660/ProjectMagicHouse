using Misc.EditorHelpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using VRPortalToolkit;
using VRPortalToolkit.Physics;
using VRPortalToolkit.Portables;


public class PortableSource : MonoBehaviour
{
    private Portable _portable;
    public Portable portable
    {
        get
        {
            if (!_portable)
            {
                _portable = GetComponent<Portable>();
                if (_portable) _portable.enabled = !_source;
            }

            return _portable;
        }
    }

    [SerializeField] private Transform _source;
    public Transform source
    {
        get => _source;
        set {
            if (_source != value)
            {
                if (isActiveAndEnabled && Application.isPlaying)
                {
                    RemoveSourceListener(_source);
                    Validate.UpdateField(this, nameof(_source), _source = value);
                    AddSourceListener(_source);
                }
                else
                    Validate.UpdateField(this, nameof(_source), _source = value);
            }
        }
    }

    public UnityEvent<Portal> failed;

    protected virtual void AddSourceListener(Transform source)
    {
        if (portable) _portable.enabled = false;

        if (source) PortalPhysics.AddPostTeleportListener(source, SourcePostTeleport);
    }

    protected virtual void RemoveSourceListener(Transform source)
    {
        if (source) PortalPhysics.RemovePostTeleportListener(source, SourcePostTeleport);
        
        if (portable) _portable.enabled = true;
    }

    protected virtual void OnValidate()
    {
        Validate.FieldWithProperty(this, nameof(_source), nameof(source));
    }

    protected virtual void OnEnable()
    {
        AddSourceListener(source);
    }

    protected virtual void OnDisable()
    {
        RemoveSourceListener(source);
    }

    protected virtual void SourcePostTeleport(Teleportation args)
    {
        // 디버그 1: 이벤트 자체가 들어오는지 확인
        Debug.Log($"[PortableDebug] {source.name}이(가) 텔레포트됨! 타겟 확인: {args.target.name}");

        if (args.target != transform && args.fromPortal)
        {
            if (!portable || _portable.IsValid(args.fromPortal))
            {
                // 디버그 2: 실제 텔레포트 명령이 내려지는지 확인
                Debug.Log("[PortableDebug] 카메라(CenterEyeAnchor) 텔레포트 실행!");
                PortalPhysics.Teleport(transform, args.fromPortal);
            }
            else
            {
                Debug.LogWarning("[PortableDebug] 포탈이 유효하지 않아 카메라 이동 실패.");
                if (failed != null) failed.Invoke(args.fromPortal);
            }
        }
        else
        {
            Debug.Log("[PortableDebug] 조건 불일치: 이미 내 몸이거나 포탈 정보가 없음.");
        }
    }
}
