using Oculus.Interaction;
using Photon.Pun;
using UnityEngine;

public class PhotoGrabSync : GrabSync
{
    public override void OnGrabEvent()
    {
        // 1. 사진 전용 로직: 카메라에서 독립하기
        if (transform.parent != null)
        {
            transform.SetParent(null);
            rb.isKinematic = false;
        }

        // 2. 부모(GrabSync)의 네트워크 동기화 로직 실행
        base.OnGrabEvent();
    }
}

