using UnityEngine;
using UnityEngine.VFX;
using System.Collections;

public class PortalVFXEffectHandler : MonoBehaviour
{
    private VisualEffect vfx;
    private Vector3 originalHandlerScale;

    [Header("Portal Settings")]
    [SerializeField] private Vector3 targetMeshScale = new Vector3(1f, 2f, 1f); // 인스펙터에서 수정 가능

    // 포탈마다 고유의 속도를 주고 싶다면 추가
    [SerializeField] private float defaultDuration = 1.0f;

    [HideInInspector] public GameObject portalDisplayMesh;

    private void Awake()
    {
        vfx = GetComponent<VisualEffect>();
        // 초기 스케일을 저장해두어 나중에 원복할 때 사용
        originalHandlerScale = transform.localScale;
        gameObject.SetActive(false);
    }

    public void PlayExpand(float duration, GameObject displayMesh = null)
    {
        gameObject.SetActive(true);

        if (displayMesh != null)
        {
            portalDisplayMesh = displayMesh;
            portalDisplayMesh.SetActive(true);
            portalDisplayMesh.transform.localScale = Vector3.zero;
        }

        StopAllCoroutines();
        StartCoroutine(ExpandRoutine(duration));
    }

    private IEnumerator ExpandRoutine(float duration)
    {
        float elapsed = 0;
        vfx.Play();

        // 0으로 시작 보장
        transform.localScale = Vector3.zero;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // 커브를 넣고 싶다면 t = Mathf.SmoothStep(0, 1, t); 같은 코드를 추가할 수 있습니다.

            // 1. VFX 핸들러 본체 확장
            transform.localScale = Vector3.Lerp(Vector3.zero, originalHandlerScale, t);

            // 2. 포탈 면 확장 (설정된 targetMeshScale 적용)
            if (portalDisplayMesh != null)
            {
                portalDisplayMesh.transform.localScale = Vector3.Lerp(Vector3.zero, targetMeshScale, t);
            }

            yield return null;
        }

        transform.localScale = originalHandlerScale;
        if (portalDisplayMesh != null) portalDisplayMesh.transform.localScale = targetMeshScale;
    }

    public void StopWithFade(float stayTime, float fadeTime)
    {
        StartCoroutine(NaturalFadeOutRoutine(stayTime, fadeTime));
    }

    private IEnumerator NaturalFadeOutRoutine(float stayTime, float fadeTime)
    {
        yield return new WaitForSeconds(stayTime);
        vfx.Stop();

        // 부드럽게 작아지며 사라지는 연출을 원한다면 여기에 Shrink 루틴을 추가할 수도 있습니다.

        yield return new WaitForSeconds(fadeTime);
        gameObject.SetActive(false);
    }
}