using Oculus.Interaction;
using Photon.Pun;
using UnityEngine;

public class ItemKey : MonoBehaviourPun, IItem
{
    // 인스펙터에서 "Key_Red"라고 적어주면 됨
    [SerializeField] 
    private string _itemID;
    [SerializeField]
    private IItem.ItemType ItemType;
    [SerializeField]
    private Rigidbody rigidbody = null;
    [SerializeField]
    private Collider collider = null;
    private int photonViewID; 

    // 인터페이스 구현: 매니저가 물어보면 이 ID를 줍니다.

    public string ItemID => _itemID;
    public IItem.ItemType Type => ItemType;
    public Transform Transform => this.transform;
    public int PhotonViewID => photonViewID;

    private void Awake()
    {
        photonViewID = photonView.ViewID;
    }

    // 성공했을 때 리시버가 호출 할 메소드
    public void OnPlaced()
    {
        IInteractable[] allInteractable = GetComponentsInChildren<IInteractable>();

        foreach (IInteractable interactable in allInteractable)
        {
            interactable.Disable();
        }

        rigidbody.isKinematic = true;
        collider.isTrigger = true;
        rigidbody.useGravity = false;
    }
}