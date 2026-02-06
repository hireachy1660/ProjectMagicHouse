using UnityEngine;
using Photon.Pun;

public class PuzzleManager : MonoBehaviourPun
{
    [Header("소환 설정")]
    public string smallDoorPrefabName = "SmallDoor_Prefab"; // Resources 폴더 내 프리팹 이름
    public Transform bPlayerSpawnPoint; // B플레이어 근처 소환 위치

    // A가 퍼즐을 풀었을 때 호출될 함수
    public void OnPuzzleCompleted()
    {
        if (PhotonNetwork.IsMasterClient) // 중복 생성을 막기 위해 마스터가 소환
        {
            // B플레이어 근처에 작은 문 생성 (포톤 네트워크 동기화)
            PhotonNetwork.Instantiate(smallDoorPrefabName, bPlayerSpawnPoint.position, Quaternion.identity);
        }
    }
}