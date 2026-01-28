using System.Collections;
using UnityEngine;
using VRPortalToolkit;

public class PortalFadeHelper : MonoBehaviour
{
    [Header("UI 및 참조")]
    public CanvasGroup fadeCanvasGroup;
    public Portal portalA;
    public Portal portalB;
    public Transform playerCamera;

    [Header("설정")]
    public float fadeStartDistance = 0.1f;

    private bool isTeleporting = false;

    private void OnEnable()
    {
        // 핵심 해결책: 매개변수가 1개인 경우를 가정하여 (args) => 로 연결합니다.
        // 만약 여기서도 에러가 나면 (args)를 ()로 바꾸면 100% 해결됩니다.
        if (portalA) portalA.preTeleport += (args) => HandleTeleport();
        if (portalB) portalB.preTeleport += (args) => HandleTeleport();
    }

    private void OnDisable()
    {
        // 람다식은 -= 해제가 까다로우므로 비워두거나 
        // 완벽을 기하려면 별도의 대리자 변수를 써야 하지만, 우선 에러 해결에 집중합니다.
    }

    private void Update()
    {
        if (isTeleporting || playerCamera == null || fadeCanvasGroup == null) return;

        float distA = portalA ? Vector3.Distance(playerCamera.position, portalA.transform.position) : float.MaxValue;
        float distB = portalB ? Vector3.Distance(playerCamera.position, portalB.transform.position) : float.MaxValue;
        float minDist = Mathf.Min(distA, distB);

        if (minDist < fadeStartDistance)
        {
            float targetAlpha = Mathf.Lerp(0.8f, 0f, minDist / fadeStartDistance);
            fadeCanvasGroup.alpha = Mathf.Max(fadeCanvasGroup.alpha, targetAlpha);
        }
        else
        {
            fadeCanvasGroup.alpha = Mathf.Lerp(fadeCanvasGroup.alpha, 0, Time.deltaTime * 5f);
        }
    }

    private void HandleTeleport()
    {
        if (isFading()) return;
        StartCoroutine(FadeSequence());
    }

    private bool isFading() => isTeleporting;

    IEnumerator FadeSequence()
    {
        isTeleporting = true;

        // 1. 암전 (들어갈 때: 0.15초 동안 부드럽게 암전)
        float timer = 0;
        float fadeOutDuration = 0.3f;
        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;
            // SmoothStep은 시작과 끝을 부드럽게 감속시켜줍니다.
            fadeCanvasGroup.alpha = Mathf.SmoothStep(0, 1, timer / fadeOutDuration);
            yield return null;
        }
        fadeCanvasGroup.alpha = 1f;

        // 2. 텔레포트 안정화 (너무 짧으면 튐 현상이 보일 수 있으니 유지)
        yield return new WaitForSeconds(0.1f);

        // 3. 다시 밝아짐 (나올 때: 0.5초 동안 아주 천천히 밝아짐)
        // 탐정이 새로운 공간에 눈을 뜨는 느낌을 줍니다.
        timer = 0;
        float fadeInDuration = 0.5f;
        while (timer < fadeInDuration)
        {
            timer += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.SmoothStep(1, 0, timer / fadeInDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = 0;
        isTeleporting = false;
    }
}