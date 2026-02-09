using UnityEngine;
using Oculus.Interaction;
using Photon.Pun;

public class VerticalRecursion : MonoBehaviourPun
{
    public Transform giantSpawnPoint;
    public float scaleMultiplier = 10f;
    public string giantDoorPrefabName = "BigDoor_Prefab";
    public bool isDoorSpawned = false; // 문 중복 생성 방지

    private void OnTriggerEnter(Collider other)
    {
        // ItemKey로 '403'문 소하ㅗㄴ
        ItemKey item = other.GetComponentInParent<ItemKey>();
        if (item != null && item.ItemID == "403" && !isDoorSpawned)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                SpawnGiantObject(item.gameObject);
                isDoorSpawned = true;
            }
        }
    }
    void SpawnGiantObject(GameObject smallObj)
    {
        // 1. 거대 문 소환
        GameObject giantObj = PhotonNetwork.Instantiate(giantDoorPrefabName, giantSpawnPoint.position, giantSpawnPoint.rotation);

        // 2. 이름 통일 (동기화 암호)
        giantObj.name = "EscapeDoor_Linked";
        smallObj.name = "EscapeDoor_Linked";

        // 3. 중요: 거대 문을 '현실 집' 부모 밑으로 넣기
        // WorldSynchronizer가 realWorldParent의 자식들만 뒤져서 등록하기 때문입니다.
        WorldSynchronizer sync = Object.FindFirstObjectByType<WorldSynchronizer>();
        if (sync != null)
        {
            giantObj.transform.SetParent(sync.realWorldParent);

            // 4. 작은 문을 '미니어처 집' 부모 밑으로 넣기
            smallObj.transform.SetParent(sync.transform);

            // 5. 이제 목록 새로고침 (밑줄 안 생기는 버전)
            sync.RefreshObjectList();
        }

        // 6. 거대 문 잡기 기능 끄기
        var grab = giantObj.GetComponentInChildren<Grabbable>();
        if (grab != null) grab.enabled = false;
    }
}