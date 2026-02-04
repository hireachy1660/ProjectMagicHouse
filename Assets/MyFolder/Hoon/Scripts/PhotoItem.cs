using Oculus.Interaction;
using Photon.Pun;
using UnityEngine;

public class PhotoItem : MonoBehaviourPun, IItem
{
    [SerializeField] private string _photoID; // PortalManager의 destinations에 설정한 photoID와 일치해야 함
    [SerializeField] private IItem.ItemType itemType = IItem.ItemType.Evidence;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Collider col;

    // 인터페이스 구현
    public string ItemID => _photoID;
    public IItem.ItemType Type => itemType;
    public Transform Transform => this.transform;
    public int PhotonViewID => photonView.ViewID;

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (col == null) col = GetComponent<Collider>();
    }

    // 포탈 생성에 성공하여 사진이 '사용'되었을 때 호출됩니다.
    public void OnPlaced()
    {
        // 1. 더 이상 잡을 수 없도록 인터랙션 비활성화
        IInteractable[] allInteractables = GetComponentsInChildren<IInteractable>();
        foreach (IInteractable interactable in allInteractables)
        {
            interactable.Disable();
        }

        // 2. 물리 효과 제거 및 트리거화
        rb.isKinematic = true;
        col.isTrigger = true;
        rb.useGravity = false;

        // 3. [낭만 연출] 여기서 사진이 서서히 사라지거나 포탈 속으로 빨려 들어가는 연출을 추가할 수 있습니다.
        // gameObject.SetActive(false); // 일단은 즉시 비활성화
    }
}