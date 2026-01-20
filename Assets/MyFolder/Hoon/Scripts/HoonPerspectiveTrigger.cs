using UnityEngine;

public class PerspectiveTrigger : MonoBehaviour
{
    public Transform playerCamera;   // Main Camera
    public Transform solutionPoint;  // 정답 위치/각도 (빈 오브젝트)
    public GameObject portalWindow;  // 나타날 사각형(Quad)

    public float posThreshold = 0.2f; // 위치 오차 (적을수록 정교함)
    public float rotThreshold = 0.99f; // 각도 오차 (1에 가까울수록 정교함)

    void Start()
    {
        portalWindow.SetActive(false); // 처음엔 꺼둠
    }

    void Update()
    {
        // 1. 위치 거리 계산
        float dist = Vector3.Distance(playerCamera.position, solutionPoint.position);
        // 2. 바라보는 각도(내적) 계산
        float dot = Vector3.Dot(playerCamera.forward, solutionPoint.forward);

        if (dist < posThreshold && dot > rotThreshold)
        {
            if (!portalWindow.activeSelf)
            {
                portalWindow.SetActive(true); // 정답이면 창문 활성화!
                Debug.Log("사각형 완성!");
            }
        }
        else
        {
            // 정답 구역을 벗어나면 다시 끌지 말지는 기획에 따라 선택하세요.
            // portalWindow.SetActive(false); 
        }
    }
}