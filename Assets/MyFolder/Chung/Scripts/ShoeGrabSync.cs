using UnityEngine;
using Photon.Pun;
using System.Collections;

public class ShoeGrabSync : GrabSync
{
    [SerializeField]
    private PhotonTransformView transformView;

    public override void OnGrabEvent()
    {
        // 1. 소유권 먼저 확보 (송신자가 되기 위함)
        if (!photonView.IsMine) photonView.RequestOwnership();

        // 2. [핵심] All로 쏴서 나를 포함한 모두의 컴포넌트를 깨움
        photonView.RPC(nameof(RPC_ShoeOnGrab), RpcTarget.AllBuffered);
    }

    [PunRPC]
    protected virtual void RPC_ShoeOnGrab()
    {
        // 송/수신측 공통 로직
        // 1. 공통: 모든 클라이언트에서 패킷 흐름을 개방합니다.
        if (transformView != null) transformView.enabled = true;

        // 2. 물리 설정: 잡은 사람은 kinematic을 풀지만,
        // 원격 플레이어들은 위치 동기화를 위해 kinematic을 켬
        if (photonView.IsMine)
        {
            if (rb.useGravity) rb.isKinematic = false;
        }
        else
        {
            rb.isKinematic = true;
            // 타인의 키내매틱을 꺼서 물건을 뺏어가지 못함
            interactable.Disable();
        }
    }

    public override void DisGrabEvent()
    {
        // 놓는 순간에는 소유자만 정지 판정 코루틴을 돌림
        if (photonView.IsMine)
        {
            StartCoroutine(ShoeStopCheckRoutine());
        }
    }

    // 플레이어가 놓았을 때 트랜스폼 뷰를 끄기위한 검사용 코루틴
    private IEnumerator ShoeStopCheckRoutine()
    {
        yield return new WaitForSeconds(0.5f); // 던진 직후 대기
                                               // 멈출 때까지(Sleep 상태가 아닐 때까지) 0.2초마다 체크하며 대기
        while (!rb.IsSleeping())   
        {
            yield return new WaitForSeconds(0.2f);
        }
        if (photonView.IsMine)
        {
        photonView.RPC(nameof(RPC_ShoeOnStop), RpcTarget.AllBuffered, transform.position, transform.rotation);
        }
        //photonView.RPC(nameof(RPC_ShoeOnStop), RpcTarget.AllBuffered);
    }

    // 최적화를 위해 더이상 움직이지 않는 오브젝트의 트랜스폼 뷰, 인터렉터블, 리지드바디를 끔
    [PunRPC]
    //protected virtual void RPC_ShoeOnStop()
    protected virtual void RPC_ShoeOnStop(Vector3 finalPos, Quaternion finalRot)
    {
        // 모든 클라이언트에서 동일하게 위치를 맞추고 수면 모드 진입
        transform.position = finalPos;
        transform.rotation = finalRot;

        rb.isKinematic = true;
        if (transformView != null) transformView.enabled = false; 
        interactable.Enable();
    }
}
