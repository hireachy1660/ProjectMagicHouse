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