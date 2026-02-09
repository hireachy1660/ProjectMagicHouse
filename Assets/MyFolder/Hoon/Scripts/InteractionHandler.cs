using UnityEngine;
using System.Collections;

public class InteractionHandler : MonoBehaviour
{
    [Header("Settings")]
    public float maxDistance = 2.5f;    // 하이라이트가 작동할 최대 거리
    public LayerMask interactableLayer; // 아이템들이 속한 레이어 (Item)
    public float checkInterval = 0.1f;  // 성능 최적화: 0.1초마다 체크 (매 프레임 X)

    private Transform mainCam;
    private Outline lastHighlighted;   // 직전에 하이라이트 된 오브젝트 저장

    void Start()
    {
        mainCam = Camera.main.transform; //
        // 성능 최적화: Update 대신 주기적 루틴 실행
        StartCoroutine(InteractionCheckRoutine());
    }

    IEnumerator InteractionCheckRoutine()
    {
        while (true)
        {
            CheckForInteractable();
            yield return new WaitForSeconds(checkInterval);
        }
    }

    void CheckForInteractable()
    {
        RaycastHit hit;
        // 1. 시선 체크
        if (Physics.Raycast(mainCam.position, mainCam.forward, out hit, maxDistance, interactableLayer))
        {
            Outline currentOutline = hit.collider.GetComponent<Outline>();

            if (currentOutline != null)
            {
                if (lastHighlighted != currentOutline)
                {
                    ClearLastHighlight();

                    // [수정] 코드로 색상/두께/모드를 정하지 않습니다.
                    // 이제 인스펙터(Outline 컴포넌트)에 설정된 값이 그대로 적용됩니다.

                    currentOutline.enabled = true;
                    lastHighlighted = currentOutline;
                }
                return;
            }
        }
        ClearLastHighlight();
    }

    void ClearLastHighlight()
    {
        if (lastHighlighted != null)
        {
            lastHighlighted.enabled = false;
            lastHighlighted = null;
        }
    }
}