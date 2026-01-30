using Oculus.Interaction;
using Photon.Pun;
using UnityEngine;

public class GrabSync : MonoBehaviourPun
{
    [SerializeField]
    private Rigidbody rb = null;
    [SerializeField]
    private GrabInteractable interactable = null;

    private void Start()
    {
        if (rb == null || interactable == null)
        {
            Debug.LogError($"{this.gameObject.name}[GrabSync]Inspector Is UnConnected");
            this.enabled = false;
        }
    }

    public void OnGrabEvent()
    {
        if(!photonView.IsMine)
        {
            photonView.RequestOwnership();
        }
        photonView.RPC(nameof(OnGrab), RpcTarget.OthersBuffered);
    }

    public void DisGrabEvent()
    {
        photonView.RPC(nameof(DisGrab), RpcTarget.OthersBuffered);
    }

    [PunRPC]
    private void OnGrab()
    {
        rb.isKinematic = true;
        interactable.Disable();
    }

    [PunRPC]
    private void DisGrab()
    {
        if (rb.useGravity == true)
        {
        rb.isKinematic = false;
        }
        interactable.Enable();
    }

    public void InitializeState(bool isKinematic, bool useGravity, bool canInteract)
    {
        // 1. 물리 상태 설정 (안착 상태인지, 자유 상태인지)
        if (rb != null)
        {
            rb.isKinematic = isKinematic;
            rb.useGravity = useGravity; // 탐정님의 '안착 지문' 전략 적용
        }

        // 2. 상호작용 가능 여부 설정
        if (interactable != null)
        {
            if (canInteract) interactable.Enable();
            else interactable.Disable();
        }

        Debug.Log($"{gameObject.name}의 초기 상태 설정 완료: Kinematic({isKinematic}), Gravity({useGravity})");
    }

}
