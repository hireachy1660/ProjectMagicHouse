using UnityEngine;
using System.Collections.Generic;

public class PortalManager : MonoBehaviour, IReceiver
{
    [System.Serializable]
    public struct DestinationData
    {
        public string photoID;
        public Transform targetPos;
    }

    [Header("Portal Settings")]
    public GameObject portalPrefab;
    public Transform entranceSpawnPoint; // [중요] 인스펙터에서 문 위치를 꼭 넣어주세요!
    public List<DestinationData> destinations;
    public Transform playerRig; // [중요] VR 카메라 리그를 넣어주세요!

    private GameObject activePortalA;
    private GameObject activePortalB;

    [Header("Test Settings")]
    public string testPhotoID = "301";

    [ContextMenu("Debug Spawn Portal")]
    public void TestSpawn()
    {
        Debug.Log("<color=yellow><b>[Test]</b> 포탈 생성을 시작합니다.</color>");
        ExecutePortalOpening(testPhotoID);
    }

    public void OnReceiveItem(IItem item)
    {
        ExecutePortalOpening(item.ItemID);
        item.OnPlaced();
    }

    private void ExecutePortalOpening(string photoID)
    {
        DestinationData data = destinations.Find(x => x.photoID == photoID);

        // 대상 위치가 비어있거나, 입구 스폰 지점이 할당되지 않았을 때의 방어 코드
        if (data.targetPos == null)
        {
            Debug.LogError($"[PortalManager] ID {photoID}에 해당하는 목적지(Target Pos)가 설정되지 않았습니다!");
            return;
        }
        if (entranceSpawnPoint == null)
        {
            Debug.LogError("[PortalManager] Entrance Spawn Point가 인스펙터에서 할당되지 않았습니다!");
            return;
        }

        // 기존 포탈 정리
        if (activePortalA) Destroy(activePortalA);
        if (activePortalB) Destroy(activePortalB);

        // 1. 입구 포탈(A) 소환
        activePortalA = Instantiate(portalPrefab, entranceSpawnPoint.position, entranceSpawnPoint.rotation);
        activePortalA.name = "Portal_Entrance_A";

        // 2. 출구 포탈(B) 소환
        activePortalB = Instantiate(portalPrefab, data.targetPos.position, data.targetPos.rotation);
        activePortalB.name = "Portal_Exit_B";

        // 3. 두 포탈 연결 로직 실행
        LinkPortals(activePortalA, activePortalB);
    }

    private void LinkPortals(GameObject a, GameObject b)
    {
        // 자식 오브젝트들로부터 컴포넌트 추출
        ModernPortal vA = a.GetComponentInChildren<ModernPortal>();
        ModernPortal vB = b.GetComponentInChildren<ModernPortal>();
        Teleporter tA = a.GetComponentInChildren<Teleporter>();
        Teleporter tB = b.GetComponentInChildren<Teleporter>();

        if (vA == null || vB == null || tA == null || tB == null)
        {
            Debug.LogError("[PortalManager] 프리팹 내부에서 ModernPortal 또는 Teleporter를 찾을 수 없습니다!");
            return;
        }

        // 비주얼(카메라) 엇갈려 연결: A는 B의 카메라를, B는 A의 카메라를
        vA.Link(vB, playerRig);
        vB.Link(vA, playerRig);

        // 물리(텔레포트) 엇갈려 연결: A의 목적지는 B, B의 목적지는 A
        tA.receiver = tB.transform;
        tB.receiver = tA.transform;

        // 필수 참조 할당 (플레이어 위치 및 메인 카메라)
        tA.playerRig = playerRig;
        tB.playerRig = playerRig;

        Transform mainCam = Camera.main.transform;
        tA.mainCamera = mainCam;
        tB.mainCamera = mainCam;

        Debug.Log("<color=cyan><b>[Portal Success]</b> 입구와 출구가 성공적으로 교차 연결되었습니다!</color>");
    }

    public void OnActivate() { }
}