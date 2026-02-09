using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;
using Oculus.Interaction;

public class PuzzleBoxManager : MonoBehaviourPun
{
    [Header("Sound Events")]
    public SoundEventSO failsound;

    [Header("퍼즐 정답 순서 (ID와 정확히 일치해야 함)")]
    public List<string> correctOrder = new List<string>() { "400", "knife", "key" };

    private List<string> currentOrder = new List<string>();

    [Header("결과물 설정")]
    //public GameObject doorGo = null;
    public string doorPrefabName = "SmallDoor"; // Resources 폴더 내 이름
    public Transform spawnPoint;

    private bool puzzleSolved = false;

    private void OnTriggerEnter(Collider other)
    {
        if (puzzleSolved) return;

        // 1. ItemKey 컴포넌트 찾기 (부모까지 확인)
        ItemKey itemKey = other.GetComponentInParent<ItemKey>();
        if (itemKey == null) return;

        // 2. 현재 잡고 있는 중인지 체크 (Oculus Grabbable 연동)
        Grabbable grab = other.GetComponentInParent<Grabbable>();
        if (grab != null && grab.SelectingPointsCount > 0) // SelectingPointsCount가 0보다 크면 잡힌 상태
            return;

        // 3. 이미 정답 처리된 물건인지(isKinematic) 확인하여 중복 방지
        if (other.attachedRigidbody != null && other.attachedRigidbody.isKinematic)
            return;

        string incomingID = itemKey.ItemID; // ItemKey.cs의 프로퍼티 이름 사용
        Debug.Log("상자에 들어온 아이템: " + incomingID);

        AddAndCheckOrder(incomingID, itemKey);
    }

    void AddAndCheckOrder(string id, ItemKey item)
    {
        int nextIndex = currentOrder.Count;

        // 현재 순서에 맞는 아이템인지 확인
        if (id == correctOrder[nextIndex])
        {
            currentOrder.Add(id);
            Debug.Log($"<color=green>정답!</color> ({currentOrder.Count}/{correctOrder.Count})");

            // 물건을 박스에 고정 (다시 못 잡게 비활성화 처리)
            item.OnPlaced();

            if (currentOrder.Count == correctOrder.Count)
            {
                SolvePuzzle();
            }
        }
        else
        {
            failsound?.PlayLocal(photonView.ViewID);
            Debug.Log("<color=red>순서 틀림! 초기화됨.</color>");
            currentOrder.Clear();
            // 틀렸을 때의 연출(예: 소리)
        }
    }

    void SolvePuzzle()
    {
        puzzleSolved = true;
        Debug.Log("모든 퍼즐 해결! 문 소환!");

        // 멀티플레이어 동기화 소환 (방장만 실행)
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Instantiate(doorPrefabName, spawnPoint.position, spawnPoint.rotation);
            //doorGo.SetActive(true);
        }
    }
}