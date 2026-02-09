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
        // 1. 카메라 정면으로 레이를 쏨 (시선 체크)
        if (Physics.Raycast(mainCam.position, mainCam.forward, out hit, maxDistance, interactableLayer))
        {
            // 찾은 물체에서 Outline 컴포넌트를 가져옴
            Outline currentOutline = hit.collider.GetComponent<Outline>();

            if (currentOutline != null)
            {
                if (lastHighlighted != currentOutline)
                {
                    ClearLastHighlight();

                    // [핵심 수정] 
                    // 1. 색상: RGB 값을 조금 더 연하게(White에 가깝게) 섞고 
                    // 2. 투명도(Alpha): 0.5f 정도로 낮춰서 '푸르스름한' 느낌 연출
                    Color softBluePurple = new Color(0.6f, 0.6f, 1f, 0.5f);

                    currentOutline.OutlineColor = softBluePurple;

                    // 두께를 5.0보다 조금 낮추면(3.0~4.0) 경계선이 더 부드러워집니다.
                    currentOutline.OutlineWidth = 3.5f;

                    // OutlineAll 대신 OutlineVisible을 쓰면 물체 뒤에 가려진 부분은 안 나와서 더 깔끔합니다.
                    currentOutline.OutlineMode = Outline.Mode.OutlineVisible;

                    currentOutline.enabled = true;
                    lastHighlighted = currentOutline;
                }
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