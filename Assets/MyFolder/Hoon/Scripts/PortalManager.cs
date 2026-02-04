using UnityEngine;
using System.Collections;
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
    public Transform entranceSpawnPoint;
    public List<DestinationData> destinations;
    public Transform playerRig;

    [Header("Door Visuals & State")]
    public GameObject doorVisual;    // 거실의 원래 문
    public bool isPortalOpened = false; // [핵심] 이 문에 포탈이 열려있는지 여부

    [Header("Romantic Animation Settings")]
    public float attachDuration = 0.6f; // 사진이 문에 붙는 시간
    public float expandDuration = 2f; // 사진이 커지는 시간
    public Vector3 targetPhotoScale = new Vector3(1f, 2f, 1f); // 문 크기에 맞춘 사진 스케일

    private GameObject activePortalA;
    private GameObject activePortalB;

    // HandManager가 호출하는 함수
    public void OnReceiveItem(IItem item)
    {
        // 1. [사용자 의도] 이미 이 문에 포탈이 열려있다면 무시
        if (isPortalOpened)
        {
            Debug.LogWarning($"<color=red>[Portal] {gameObject.name}에는 이미 포탈이 존재합니다!</color>");
            return;
        }

        // 2. 낭만 연출 시퀀스 시작
        StartCoroutine(PortalOpeningSequence(item));
    }

    private IEnumerator PortalOpeningSequence(IItem item)
    {
        isPortalOpened = true; // 시작하자마자 중복 호출 방지

        // 아이템 기능 정지 (ItemKey.OnPlaced 실행)
        item.OnPlaced();
        Transform itemTF = item.Transform;

        // [연출 1] 부착: 사진이 문 중앙으로 날아감
        Vector3 startPos = itemTF.position;
        Quaternion startRot = itemTF.rotation;
        float elapsed = 0f;
        while (elapsed < attachDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / attachDuration;
            // 부드러운 이동을 위해 Lerp 사용
            itemTF.position = Vector3.Lerp(startPos, entranceSpawnPoint.position, t);
            itemTF.rotation = Quaternion.Slerp(startRot, entranceSpawnPoint.rotation, t);
            yield return null;
        }

        // [연출 2] 확대: 사진이 문 크기만큼 커짐
        elapsed = 0f;
        Vector3 startScale = itemTF.localScale;
        while (elapsed < expandDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / expandDuration;
            itemTF.localScale = Vector3.Lerp(startScale, targetPhotoScale, t);
            yield return null;
        }

        // [연출 3] 교체: 사진과 문을 끄고 포탈 소환
        itemTF.gameObject.SetActive(false);
        if (doorVisual != null) doorVisual.SetActive(false);

        // 실제 포탈 개방 로직 실행
        ExecutePortalOpening(item.ItemID);
    }

    private void ExecutePortalOpening(string photoID)
    {
        DestinationData data = destinations.Find(x => x.photoID == photoID);
        if (data.targetPos == null) return;

        // 포탈 소환
        activePortalA = Instantiate(portalPrefab, entranceSpawnPoint.position, entranceSpawnPoint.rotation);
        activePortalB = Instantiate(portalPrefab, data.targetPos.position, data.targetPos.rotation);

        LinkPortals(activePortalA, activePortalB);
        Debug.Log("<color=gold><b>[낭만 성공]</b> 포탈 개방 완료!</color>");
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
        tA.playerRig = tB.playerRig = playerRig;

        Transform mainCam = Camera.main.transform;
        tA.mainCamera = tB.mainCamera = mainCam;
    }

    // 포탈 초기화 (필요시 사용)
    public void ResetPortal()
    {
        if (activePortalA) Destroy(activePortalA);
        if (activePortalB) Destroy(activePortalB);
        if (doorVisual) doorVisual.SetActive(true);
        isPortalOpened = false;
    }

    // IReceiver 인터페이스를 유지하기 위해 필요한 빈 함수입니다.
    public void OnActivate()
    {
        // 현재는 특별한 기능이 필요 없으므로 비워둡니다.
        // 만약 리시버가 작동할 때 추가로 실행될 로직이 있다면 여기에 작성합니다.
    }
}