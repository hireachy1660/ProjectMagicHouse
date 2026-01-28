using UnityEngine;

public class Teleporter : MonoBehaviour
{
    public Transform receiver;
    public Transform playerRig;
    public Transform mainCamera;

    private bool playerIsOverlapping = false;
    private static bool isArriving = false; // 전역 상태 관리

    void Update()
    {
        if (playerIsOverlapping && !isArriving)
        {
            Vector3 portalToCamera = mainCamera.position - transform.position;
            // 이제 화살표 방향을 맞췄으니 이 로직이 정확히 작동합니다.
            float dotProduct = Vector3.Dot(transform.forward, portalToCamera);

            if (dotProduct < 0f)
            {
                ExecuteTeleport();
            }
        }
    }

    void ExecuteTeleport()
    {
        isArriving = true; // 목적지 포탈에서 즉시 튕겨 나가는 것 방지

        Vector3 m_offset = playerRig.position - transform.position;
        // 목적지 면에서 아주 살짝(0.1m) 앞으로 밀어주어 끼임 방지
        playerRig.position = receiver.position + m_offset + (receiver.forward * 0.1f);

        Debug.Log($"{gameObject.name} -> {receiver.name} 이동 완료!");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.name.Contains("Camera"))
        {
            playerIsOverlapping = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.name.Contains("Camera"))
        {
            playerIsOverlapping = false;
            isArriving = false; // 트리거를 나가야 다음 텔레포트 준비 완료
        }
    }
}