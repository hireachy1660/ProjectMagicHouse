using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class DetectiveBoardReceiver : MonoBehaviourPun, IReceiver
{
    [System.Serializable]
    public struct EvidenceSlot
    {
        public string requiredItemID; // 이 자리에 와야 할 아이템 ID (예: "Photo_Footprint", "201")
        public Transform placePoint;  // 아이템이 붙을 실제 위치 (Transform)
    }

    [Header("Evidence Setup")]
    // 인스펙터에서 증거 순서대로 등록 (0번 슬롯 -> 1번 슬롯...)
    [SerializeField] private List<EvidenceSlot> evidenceSlots;

    [Header("Effects")]
    [SerializeField] private ParticleSystem burnEffect; // 태울 때 나올 파티클 (선택)

    private int _currentIndex = 0; // 현재 채워야 할 슬롯 번호

    public void OnReceiveItem(IItem _item)
    {
        // 1. 이미 모든 증거를 다 찾았는지 확인
        if (_currentIndex >= evidenceSlots.Count)
        {
            Debug.Log(" 이미 모든 증거가 수집되었습니다.");
            return;
        }

        // 2. 현재 순서에 맞는 아이템인지 검사
        EvidenceSlot currentSlot = evidenceSlots[_currentIndex];

        if (_item.ItemID == currentSlot.requiredItemID)
        {
            //  정답: 아이템을 보드에 부착
            int viewID = _item.PhotonViewID;

            photonView.RPC(nameof(AttachEvidence), RpcTarget.AllBuffered, viewID, _currentIndex);
            //AttachEvidence(viewID, _currentIndex);

            Debug.Log($" 증거 확보 완료! ({_currentIndex}/{evidenceSlots.Count})");

            // 모든 증거를 모았을 때의 처리 (예: 게임 클리어)
            if (_currentIndex >= evidenceSlots.Count)
            {
                Debug.Log(" 사건 해결! 모든 증거를 모았습니다.");
            }
        }
        else
        {
            bool isFutureEvidence = evidenceSlots.Exists(slot => slot.requiredItemID == _item.ItemID);
            if (isFutureEvidence)
            {
                Debug.Log("[DetectiveBoardReciver] Not Current Process Evidence");
            }
            else
            {
                //  오답: 태워버리기
                Debug.Log($" 틀린 증거입니다! {_item.ItemID}를 소각합니다.");
                BurnEvidence(_item);
            }
        }
    }

    public void OnActivate()
    {
        // 보드를 그냥 클릭했을 때 힌트 출력
        if (_currentIndex < evidenceSlots.Count)
        {
            Debug.Log($"힌트: 다음 증거는 [{evidenceSlots[_currentIndex].requiredItemID}] 인 것 같아...");
        }
    }

    // --- 내부 로직 ---  PunRPC 호출해줘야 함

    [PunRPC]
    private void AttachEvidence(int _viewID, int _trIndex)
    {
        IItem item = PhotonView.Find(_viewID).gameObject.GetComponent<IItem>();
        item.OnPlaced();

        // 인터페이스를 MonoBehaviour로 형변환하여 게임 오브젝트 제어
        MonoBehaviour itemObj = item as MonoBehaviour;
        if (itemObj == null) return;

        // 3. 위치 및 회전 고정 (자식으로 넣기)
        //itemObj.transform.parent.SetParent(_point);
        //itemObj.transform.parent.localPosition = Vector3.zero;
        //itemObj.transform.parent.localRotation = Quaternion.identity;
        itemObj.transform.SetParent(evidenceSlots[_trIndex].placePoint);
        itemObj.transform.localPosition = Vector3.zero;
        itemObj.transform.localRotation = Quaternion.identity;

        _currentIndex++; // 다음 단계로 진행

    }

    // [수정] 소각 로직을 RPC로 변경
    private void BurnEvidence(IItem _item)
    {
        if (_item.Type != IItem.ItemType.Evidence) return;
        // 인터페이스에서 ViewID를 가져와 RPC 전송
        photonView.RPC(nameof(PunBurnEvidence), RpcTarget.All, _item.PhotonViewID);
    }

    [PunRPC]
    private void PunBurnEvidence(int _viewID)
    {
        PhotonView targetView = PhotonView.Find(_viewID);
        if (targetView == null) return;

        // 1. 이펙트 재생 (모든 플레이어의 화면에서)
        if (burnEffect != null)
        {
            ParticleSystem fx = Instantiate(burnEffect, targetView.transform.position, Quaternion.identity);
            fx.Play();
            Destroy(fx.gameObject, 2.0f);
        }

        // 2. 아이템 삭제 (네트워크 파괴)
        // 소유자만 파괴할 수 있으므로, 마스터 클라이언트가 파괴를 주도하는 것이 안전합니다.
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(targetView.gameObject);
        }
    }
}