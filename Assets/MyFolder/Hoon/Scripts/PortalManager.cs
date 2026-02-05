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

        StartCoroutine(PortalOpeningSequence(item));
    }

    private IEnumerator PortalOpeningSequence(IItem item)
    {
        isPortalOpened = true;
        item.OnPlaced();
        Transform itemTF = item.Transform;

        // [연출 1] 사진 부착
        yield return StartCoroutine(AttachPhotoSequence(itemTF));

        // [연출 2] 사진 부착 후 잠시 대기
        yield return new WaitForSeconds(photoFadeDelay);

        // [연출 3] 실제 포탈 및 면(Mesh) 생성
        ExecutePortalOpening(item.ItemID);

        GameObject displayMesh = null;
        if (activePortalA != null)
        {
            Transform t = activePortalA.transform.Find("Portal_view_A");
            if (t != null) displayMesh = t.gameObject;
        }

        // [연출 4] 포탈 확장 시작
        if (portalVFXHandler != null)
        {
            portalVFXHandler.gameObject.SetActive(true);

            // [수정] 사진 소멸을 시작하고, 그 코루틴이 끝날 때까지 기다리지 않고 다음 줄로 넘어갑니다.
            StartCoroutine(FadeOutPhotoOnly(itemTF, expandDuration));

            // 포탈 면이 커지는 동안 대기
            yield return StartCoroutine(ExpandRoutineForManager(expandDuration, displayMesh));
        }

        // [연출 5] 포탈 면이 다 커진 직후 문을 비활성화
        if (doorVisual)
        {
            doorVisual.SetActive(false);
            Debug.Log("포탈이 완전히 열려 문을 숨깁니다.");
        }

        // [연출 6] VFX 파티클 소멸
        if (portalVFXHandler)
        {
            portalVFXHandler.StopWithFade(vfxStayDuration, vfxFadeOutDuration);
        }
    }

    // Manager에서 Handler의 연출이 끝날 때까지 기다려주기 위한 브릿지 코루틴
    private IEnumerator ExpandRoutineForManager(float duration, GameObject mesh)
    {
        portalVFXHandler.PlayExpand(duration, mesh);
        yield return new WaitForSeconds(duration); // 확장이 끝나는 시간만큼 대기
    }

    // 사진 소멸 로직 강화
    private IEnumerator FadeOutPhotoOnly(Transform photoTF, float duration)
    {
        if (photoTF == null) yield break;

        float elapsed = 0f;
        List<Material> photoMats = GetMaterialsFromObj(photoTF.gameObject);

        // 시작 시 투명도 조절이 가능하도록 머티리얼 설정 확인 권장 (코드로는 제어 불가, 인스펙터 확인 필요)
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float alpha = Mathf.Lerp(1f, 0f, t);

            ApplyAlphaToMats(photoMats, alpha);
            yield return null;
        }

        // [핵심] 알파값 조절 후 아예 오브젝트를 비활성화하여 '자국'을 없앱니다.
        photoTF.gameObject.SetActive(false);

        // 다음 사용을 위해 투명도를 다시 1로 초기화해둡니다 (나중에 다시 나타날 때 대비)
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

    /// <summary>
    /// 포탈을 닫고 문을 다시 켭니다. 
    /// 맵 이동 후나 리셋이 필요할 때 호출하세요.
    /// </summary>
    public void ResetPortal()
    {
        if (activePortalA) Destroy(activePortalA);
        if (activePortalB) Destroy(activePortalB);
        if (portalVFXHandler) portalVFXHandler.gameObject.SetActive(false);

        // [중요] 비활성화했던 문을 다시 켭니다.
        if (doorVisual)
        {
            doorVisual.SetActive(true);
            // 투명도도 1로 복구
            ApplyAlphaToMats(GetMaterialsFromObj(doorVisual), 1f);
        }

        isPortalOpened = false;
    }

    public void OnActivate() { }
}