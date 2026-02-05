using UnityEngine;
using UnityEngine.VFX;
using System.Collections;

public class PortalVFXEffectHandler : MonoBehaviour
{
    private VisualEffect vfx;
    private Vector3 originalHandlerScale;
    // 포탈 면의 최종 목표 크기 (1, 2, 1)
    private Vector3 targetMeshScale = new Vector3(1f, 2f, 1f);

    [HideInInspector] public GameObject portalDisplayMesh;

    private void Awake()
    {
        vfx = GetComponent<VisualEffect>();
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
            // 연출 시작을 위해 크기 0으로 초기화
            portalDisplayMesh.transform.localScale = Vector3.zero;
        }

        StopAllCoroutines();
        StartCoroutine(ExpandRoutine(duration));
    }

    private IEnumerator ExpandRoutine(float duration)
    {
        float elapsed = 0;
        vfx.Play();

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // 1. VFX 핸들러 본체 확장
            transform.localScale = Vector3.Lerp(Vector3.zero, originalHandlerScale, t);

            // 2. 포탈 면 확장 (0 -> 1, 2, 1)
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
        yield return new WaitForSeconds(fadeTime);
        gameObject.SetActive(false);
    }
}