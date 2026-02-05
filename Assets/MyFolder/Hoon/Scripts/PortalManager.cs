using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun; // 추가 [cite: 2025-12-24]

public class PortalManager : MonoBehaviourPun, IReceiver // MonoBehaviourPun 상속 [cite: 2025-12-24]
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

    public void OnReceiveItem(IItem item)
    {
        if (isPortalOpened) return;

        if (!destinations.Exists(x => x.photoID == item.ItemID))
        {
            Debug.LogWarning($"ID '{item.ItemID}'에 해당하는 목적지가 없습니다.");
            return;
        }

        // [네트워크 핵심] 방에 있는 모든 사람에게 포탈 생성을 명령합니다. [cite: 2025-12-24]
        photonView.RPC("RPC_StartPortalSequence", RpcTarget.All, item.ItemID);
    }

    [PunRPC] // 모든 클라이언트에서 실행될 함수 [cite: 2025-12-24]
    private void RPC_StartPortalSequence(string itemID)
    {
        // 네트워크 환경에서는 item 오브젝트가 각자 다를 수 있으므로 ID로 처리하거나
        // 씬 내의 아이템을 찾아야 합니다. 여기선 연출을 위해 아이템 태그 등으로 찾거나
        // 상호작용한 아이템을 특정하는 로직이 필요할 수 있습니다.
        // 우선 기존 로직을 최대한 유지하며 시퀀스를 실행합니다.
        StartCoroutine(PortalOpeningSequenceByNet(itemID));
    }

    private IEnumerator PortalOpeningSequenceByNet(string itemID)
    {
        isPortalOpened = true;

        // 실제 아이템 오브젝트를 씬에서 찾아 연출에 사용 (간단한 예시)
        GameObject itemObj = GameObject.Find(itemID); // 아이템 이름이 ID와 같다고 가정
        Transform itemTF = itemObj ? itemObj.transform : null;

        if (itemTF) yield return StartCoroutine(AttachPhotoSequence(itemTF));
        yield return new WaitForSeconds(photoFadeDelay);

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

    // ... [FadeOutPhotoOnly, GetMaterialsFromObj, ApplyAlphaToMats, AttachPhotoSequence 코드는 기존과 동일] ...

    private IEnumerator ExpandRoutineForManager(float duration, GameObject mesh)
    {
        portalVFXHandler.PlayExpand(duration, mesh);
        yield return new WaitForSeconds(duration);
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

    public void ResetPortal()
    {
        // 리셋도 필요하다면 RPC로 동기화해야 합니다. [cite: 2025-12-24]
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