using UnityEngine;
using System.Collections;

public class InteractionHandler : MonoBehaviour
{
    [Header("Settings")]
    public float maxDistance = 2.5f;    // 하이라이트가 작동할 최대 거리
    public LayerMask interactableLayer; // 아이템들이 속한 레이어 (예: Item)
    public float checkInterval = 0.1f;  // 성능을 위해 0.1초마다 체크 (매 프레임 X)

    private Transform mainCam;
    private Outline lastHighlighted;   // 직전에 하이라이트 된 오브젝트 저장

    void Start()
    {
        mainCam = Camera.main.transform;
        // 성능 최적화: 매 프레임 Update 대신 코루틴 사용
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
        // 1. 카메라 정면으로 레이를 쏨 (시선 체크)
        if (Physics.Raycast(mainCam.position, mainCam.forward, out hit, maxDistance, interactableLayer))
        {
            Outline currentOutline = hit.collider.GetComponent<Outline>();

            if (currentOutline != null)
            {
                // 새로운 물체를 쳐다보고 있다면
                if (lastHighlighted != currentOutline)
                {
                    ClearLastHighlight(); // 이전 물체 끄기
                    currentOutline.enabled = true; // 새 물체 켜기
                    lastHighlighted = currentOutline;
                }
                return; // 찾았으니 종료
            }
        }

        // 아무것도 안 맞았거나 거리가 멀어지면 하이라이트 해제
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