using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;
using Oculus.Interaction;

public class PuzzleBoxManager : MonoBehaviourPun
{
    [Header("Sound Events")]
    public SoundEventSO failsound;
    public SoundEventSO successound;


    [Header("퍼즐 정답 순서 (ID와 정확히 일치해야)")]
    public List<string> correctOrder = new List<string>() { "400", "401", "404" };
    private List<string> currentOrder = new List<string>();
    private List<ItemKey> placedItems = new List<ItemKey>();

    [Header("결과물 설정")]
    public string doorID = "110";
    public string doorPrefabName = "[BuildingBlock] Cube_Key 1"; // Resources 폴더 안
    public Transform spawnPoint;

    private bool puzzleSolved = false;

    private void OnTriggerStay(Collider other)
    {
        if (puzzleSolved) return;

        // ItemKey 찾기
        ItemKey itemKey = (other.attachedRigidbody != null) ?
            other.attachedRigidbody.GetComponent<ItemKey>() : other.GetComponentInParent<ItemKey>();

        if (itemKey == null || itemKey.ItemID == doorID) return;

        // 잡기 상태 체크
        Grabbable grab = itemKey.GetComponentInParent<Grabbable>();
        if (grab != null && grab.SelectingPointsCount > 0)
        {
            Debug.Log($"[ID: {itemKey.ItemID}] 현재 손으로 잡고 있음 - 대기");
            return;
        }

        // 중복 체크
        if (currentOrder.Contains(itemKey.ItemID))
        {
            Debug.Log($"[ID: {itemKey.ItemID}] 이미 등록됨 - 스킵");
            return;
        }

        // 정답 체크 진입
        Debug.Log($"<color=yellow>[ID: {itemKey.ItemID}] 박스 안 안착! 검사 시작</color>");
        AddAndCheckOrder(itemKey.ItemID, itemKey);
    }

    void AddAndCheckOrder(string id, ItemKey item)
    {
        int nextIndex = currentOrder.Count;

        // 다음 순서가 맞는지 체크
        if (nextIndex < correctOrder.Count && id == correctOrder[nextIndex])
        {
            currentOrder.Add(id);
            placedItems.Add(item);
            Debug.Log($"<color=green> 정답!</color> 현재 진행: ({currentOrder.Count}/{correctOrder.Count})");

            // 3개 모두 정답으로 들어갔을 때만
            if (currentOrder.Count == correctOrder.Count)
            {
                Debug.Log("<color=cyan> 퍼즐 완료! 아이템 고정 + 문 소환 </color>");

                // 1. 아이템 모두 고정
                foreach (var placedItem in placedItems)
                {
                    if (placedItem != null)
                    {
                        placedItem.OnPlaced();
                        Debug.Log($"<color=white>[ID: {placedItem.ItemID}] 고정 완료</color>");
                    }
                }

                // 2. 문 소환
                SolvePuzzle();
            }
        }
        else
        {
            failsound?.PlayLocal(photonView.ViewID);
            Debug.Log($"<color=red> 순서 틀림!</color> {nextIndex + 1}번째는 '{correctOrder[nextIndex]}'여야 하는데 '{id}' 들어옴");
            Debug.Log("<color=orange> 순서 리셋! 다시 넣으세요</color>");
            currentOrder.Clear();
            placedItems.Clear();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (puzzleSolved) return;

        ItemKey itemKey = (other.attachedRigidbody != null) ?
            other.attachedRigidbody.GetComponent<ItemKey>() : other.GetComponentInParent<ItemKey>();

        if (itemKey != null && currentOrder.Contains(itemKey.ItemID))
        {
            Debug.Log($"<color=magenta>[ID: {itemKey.ItemID}] 박스에서 나감! 순서 리셋</color>");
            currentOrder.Clear();
            placedItems.Clear();
        }
    }

    void SolvePuzzle()
    {

        puzzleSolved = true;
        Debug.Log("<color=cyan> 문 소환 시도!</color>");
        successound?.PlayLocal(photonView.ViewID);

        GameObject doorPrefab = Resources.Load<GameObject>(doorPrefabName);

        if (doorPrefab == null)
        {
            Debug.LogError($"<color=red> Resources/{doorPrefabName} 프리팹 없음!</color>");
            return;
        }

        // 포톤 연결 여부 체크해서 자동 선택
        if (PhotonNetwork.IsConnected && doorPrefab.GetComponent<PhotonView>() != null)
        {
            PhotonNetwork.Instantiate(doorPrefabName, spawnPoint.position, spawnPoint.rotation);
            Debug.Log("<color=green> 포톤으로 문 생성 완료!</color>");
        }
        else
        {
            Instantiate(doorPrefab, spawnPoint.position, spawnPoint.rotation);
            Debug.Log("<color=green> 일반 Instantiate로 문 생성 완료!</color>");
        }
    }
}