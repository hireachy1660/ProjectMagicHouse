using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;

public class PortalManager : MonoBehaviourPun, IReceiver
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
    public GameObject doorVisual;
    public bool isPortalOpened = false;

    [Header("Animation Settings")]
    public float attachDuration = 1f;
    public float photoFadeDelay = 0.5f;
    public float expandDuration = 1.5f;
    public float vfxStayDuration = 2.0f;
    public float vfxFadeOutDuration = 1.0f;

    [Header("VFX Settings")]
    public PortalVFXEffectHandler portalVFXHandler;

    private GameObject activePortalA;
    private GameObject activePortalB;

    [Header("Sound Events")]
    public SoundEventSO attachSound;
    public SoundEventSO portalOpenSound;

    [Header("SO Reference")]
    public GameStatusSO gameStatus; // 인스펙터에서 GameStatusSO 파일을 연결해주세요.

    private void Update()
    {
        // 1. 포탈이 열려있고, X 버튼을 눌렀을 때
        if (isPortalOpened && OVRInput.GetDown(OVRInput.RawButton.X))
        {
            // 2. SO에서 현재 나의 직업(myRole)을 확인합니다.
            if (gameStatus == null)
            {
                Debug.LogError("GameStatusSO가 연결되지 않았습니다!");
                return;
            }

            // SO의 myRole이 "Pathfinder"인지 확인합니다.
            if (gameStatus.myRole != "Pathfinder")
            {
                Debug.Log($"<color=red>권한 부족: 당신의 직업은 {gameStatus.myRole}입니다. 패스파인더만 포탈을 닫을 수 있습니다.</color>");
                return;
            }

            // 3. 시선 체크 로직 실행
            if (IsPlayerLookingAtPortal())
            {
                RequestResetPortal();
            }
        }
    }

    // 시선 체크 로직
    private bool IsPlayerLookingAtPortal()
    {
        if (playerRig == null || entranceSpawnPoint == null) return false;

        // 메인 카메라(플레이어 눈)의 위치와 방향
        Transform camTF = Camera.main.transform;

        // 1. 플레이어 눈에서 포탈 입구로 향하는 방향 벡터 계산
        Vector3 dirToPortal = (entranceSpawnPoint.position - camTF.position).normalized;

        // 2. 카메라가 바라보는 방향(Forward)
        Vector3 lookDir = camTF.forward;

        // 3. 두 벡터 사이의 각도 계산 (0 ~ 180도)
        float angle = Vector3.Angle(lookDir, dirToPortal);

        // 4. 거리 체크도 추가하면 좋습니다 (너무 멀리서 꺼지는 것 방지)
        float distance = Vector3.Distance(camTF.position, entranceSpawnPoint.position);

        // [설정] 각도 45도 이내 AND 거리 5미터 이내일 때만 허용
        if (angle < 45f && distance < 5.0f)
        {
            Debug.Log($"<color=cyan>시선 일치! (각도: {angle:F1}, 거리: {distance:F1})</color>");
            return true;
        }

        return false;
    }



    // 1. 아이템 수신 (RPC 호출 시 viewID 추가 전송)
    public void OnReceiveItem(IItem item)
    {
        if (isPortalOpened) return;

        if (!destinations.Exists(x => x.photoID == item.ItemID))
        {
            Debug.LogWarning($"ID '{item.ItemID}'에 해당하는 목적지가 없습니다.");
            return;
        }

        // itemID(목적지 식별)와 item.PhotonViewID(물체 식별)를 함께 보냄
        photonView.RPC("RPC_StartPortalSequence", RpcTarget.All, item.ItemID, item.PhotonViewID);
    }

    // 2. RPC 함수 (인자 2개 받도록 수정)
    [PunRPC]
    private void RPC_StartPortalSequence(string itemID, int viewID)
    {
        // ViewID로 네트워크상의 정확한 오브젝트 찾기
        PhotonView targetPV = PhotonView.Find(viewID);
        GameObject itemObj = targetPV != null ? targetPV.gameObject : null;

        // 찾은 오브젝트를 시퀀스로 전달
        StartCoroutine(PortalOpeningSequenceByNet(itemID, itemObj));
    }

    // 3. 메인 시퀀스 (GameObject 직접 받아서 처리)
    private IEnumerator PortalOpeningSequenceByNet(string itemID, GameObject itemObj)
    {
        isPortalOpened = true;
        Transform itemTF = itemObj ? itemObj.transform : null;

        if (itemTF)
        {
            // [중요] 아이템의 물리와 그랩을 먼저 꺼야 문으로 날아갑니다.
            itemObj.GetComponent<IItem>()?.OnPlaced();

            attachSound?.PlayLocal(photonView.ViewID);
            yield return StartCoroutine(AttachPhotoSequence(itemTF));
        }

        yield return new WaitForSeconds(photoFadeDelay);

        portalOpenSound?.PlayLocal(photonView.ViewID);
        ExecutePortalOpening(itemID);

        GameObject displayMesh = null;
        if (activePortalA != null)
        {
            Transform t = activePortalA.transform.Find("Portal_view_A");
            if (t != null) displayMesh = t.gameObject;
        }

        if (portalVFXHandler != null)
        {
            portalVFXHandler.gameObject.SetActive(true);
            if (itemTF) StartCoroutine(FadeOutPhotoOnly(itemTF, expandDuration));
            yield return StartCoroutine(ExpandRoutineForManager(expandDuration, displayMesh));
        }

        if (doorVisual) doorVisual.SetActive(false);

        if (portalVFXHandler)
        {
            portalVFXHandler.StopWithFade(vfxStayDuration, vfxFadeOutDuration);
        }
    }

    private IEnumerator AttachPhotoSequence(Transform itemTF)
    {
        float heightOffset = 0.5f;
        float distanceOffset = -0.02f;
        Vector3 targetPos = entranceSpawnPoint.position
                            + (entranceSpawnPoint.forward * distanceOffset)
                            + (entranceSpawnPoint.up * heightOffset);

        Vector3 startPos = itemTF.position;
        Quaternion startRot = itemTF.rotation;
        float elapsed = 0f;

        while (elapsed < attachDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / attachDuration;
            itemTF.position = Vector3.Lerp(startPos, targetPos, t);
            itemTF.rotation = Quaternion.Slerp(startRot, entranceSpawnPoint.rotation, t);
            yield return null;
        }
    }

    private void ExecutePortalOpening(string photoID)
    {
        DestinationData data = destinations.Find(x => x.photoID == photoID);
        if (data.targetPos == null) return;

        if (activePortalA) Destroy(activePortalA);
        if (activePortalB) Destroy(activePortalB);

        activePortalA = Instantiate(portalPrefab, entranceSpawnPoint.position, entranceSpawnPoint.rotation);
        activePortalB = Instantiate(portalPrefab, data.targetPos.position, data.targetPos.rotation);

        LinkPortals(activePortalA, activePortalB);
    }

    private void LinkPortals(GameObject a, GameObject b)
    {
        ModernPortal vA = a.GetComponentInChildren<ModernPortal>();
        ModernPortal vB = b.GetComponentInChildren<ModernPortal>();
        Teleporter tA = a.GetComponentInChildren<Teleporter>();
        Teleporter tB = b.GetComponentInChildren<Teleporter>();

        if (vA && vB) { vA.Link(vB, playerRig); vB.Link(vA, playerRig); }
        if (tA && tB)
        {
            tA.receiver = tB.transform; tB.receiver = tA.transform;
            tA.playerRig = tB.playerRig = playerRig;
            tA.mainCamera = tB.mainCamera = Camera.main.transform;
        }
    }

    private IEnumerator FadeOutPhotoOnly(Transform photoTF, float duration)
    {
        if (photoTF == null) yield break;
        float elapsed = 0f;
        List<Material> photoMats = GetMaterialsFromObj(photoTF.gameObject);
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float alpha = Mathf.Lerp(1f, 0f, t);
            ApplyAlphaToMats(photoMats, alpha);
            yield return null;
        }
        photoTF.gameObject.SetActive(false);
        ApplyAlphaToMats(photoMats, 1f);
    }

    private List<Material> GetMaterialsFromObj(GameObject obj)
    {
        List<Material> mats = new List<Material>();
        if (obj == null) return mats;
        MeshRenderer[] renderers = obj.GetComponentsInChildren<MeshRenderer>();
        foreach (var r in renderers)
        {
            foreach (var m in r.materials) mats.Add(m);
        }
        return mats;
    }

    private void ApplyAlphaToMats(List<Material> mats, float alpha)
    {
        foreach (var mat in mats)
        {
            if (mat.HasProperty("_BaseColor"))
            {
                Color c = mat.GetColor("_BaseColor");
                c.a = alpha;
                mat.SetColor("_BaseColor", c);
            }
            else if (mat.HasProperty("_Color"))
            {
                Color c = mat.GetColor("_Color");
                c.a = alpha;
                mat.SetColor("_Color", c);
            }
        }
    }

    private IEnumerator ExpandRoutineForManager(float duration, GameObject mesh)
    {
        portalVFXHandler.PlayExpand(duration, mesh);
        yield return new WaitForSeconds(duration);
    }


    private void RequestResetPortal()
    {
        // 모든 클라이언트에게 포탈 리셋 동기화 요청
        photonView.RPC("RPC_ResetPortal", RpcTarget.All);
    }

    public void ResetPortal()
    {
        if (PhotonNetwork.IsMasterClient)
            photonView.RPC("RPC_ResetPortal", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_ResetPortal()
    {
        if (activePortalA) Destroy(activePortalA);
        if (activePortalB) Destroy(activePortalB);
        if (portalVFXHandler) portalVFXHandler.gameObject.SetActive(false);
        if (doorVisual)
        {
            doorVisual.SetActive(true);
            ApplyAlphaToMats(GetMaterialsFromObj(doorVisual), 1f);
        }
        isPortalOpened = false;
    }

    public void OnActivate() { }
}