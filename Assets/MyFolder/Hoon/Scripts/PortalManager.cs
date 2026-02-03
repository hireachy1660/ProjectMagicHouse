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

    public GameObject portalPrefab;
    public Transform entranceSpawnPoint;
    public List<DestinationData> destinations;
    public Transform playerRig;

    private GameObject activePortalA;
    private GameObject activePortalB;

    // --- 테스트를 위한 코드 추가 ---
    [Header("Test Settings")]
    public string testPhotoID = "301"; // 테스트하고 싶은 사진 ID

    [ContextMenu("Debug Spawn Portal")] // 인스펙터에서 컴포넌트 우클릭 시 나타남
    public void TestSpawn()
    {
        // 실제 아이템 없이도 ID만으로 호출 가능하게 로직 분리
        ExecutePortalOpening(testPhotoID);
    }
    // ----------------------------

    public void OnReceiveItem(IItem item)
    {
        ExecutePortalOpening(item.ItemID);
        item.OnPlaced();
    }

    private void ExecutePortalOpening(string photoID)
    {
        DestinationData data = destinations.Find(x => x.photoID == photoID);

        if (data.targetPos != null)
        {
            if (activePortalA) Destroy(activePortalA);
            if (activePortalB) Destroy(activePortalB);

            activePortalA = Instantiate(portalPrefab, entranceSpawnPoint.position, entranceSpawnPoint.rotation);
            activePortalB = Instantiate(portalPrefab, data.targetPos.position, data.targetPos.rotation);

            LinkPortals(activePortalA, activePortalB);
        }
    }

    private void LinkPortals(GameObject a, GameObject b)
    {
        ModernPortal vA = a.GetComponentInChildren<ModernPortal>();
        ModernPortal vB = b.GetComponentInChildren<ModernPortal>();
        Teleporter tA = a.GetComponentInChildren<Teleporter>();
        Teleporter tB = b.GetComponentInChildren<Teleporter>();

        vA.Link(vB, playerRig);
        vB.Link(vA, playerRig);

        tA.receiver = tB.transform;
        tB.receiver = tA.transform;
        tA.playerRig = playerRig;
        tB.playerRig = playerRig;
        tA.mainCamera = Camera.main.transform;
        tB.mainCamera = Camera.main.transform;
    }

    public void OnActivate() { }
}