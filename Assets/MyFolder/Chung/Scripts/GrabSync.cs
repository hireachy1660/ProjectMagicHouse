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

}
