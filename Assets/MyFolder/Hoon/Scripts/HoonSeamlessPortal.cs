using UnityEngine;

public class HoonSeamlessPortal : MonoBehaviour
{
    public Transform playerTarget;      // Main Camera (HMD)
    public Transform playerOrigin;      // XR Origin (전체 리그)
    public Transform portalExit;        // 가상 세계의 출구 포인트
    public Transform portalCamera;      // 가상 세계를 찍는 카메라

    private void Update()
    {
        if (playerTarget == null || portalCamera == null || portalExit == null) return;

        // 1. 플레이어가 현재 포탈(현실 혹은 가상) 면에서 어디에 있는지 계산
        Vector3 relativePos = transform.InverseTransformPoint(playerTarget.position);

        // 2. 포탈 뒤쪽으로 카메라를 배치하여 실제 공간 같은 깊이감 생성 (Z값 반전)
        Vector3 cameraOffset = new Vector3(relativePos.x, relativePos.y, -relativePos.z);

        // 3. 목적지(Exit)를 기준으로 카메라 위치와 회전 동기화
        portalCamera.position = portalExit.TransformPoint(cameraOffset);

        Quaternion relativeRot = Quaternion.Inverse(transform.rotation) * playerTarget.rotation;
        portalCamera.rotation = portalExit.rotation * relativeRot;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MainCamera") || other.CompareTag("Player"))
        {

            // 어떤 물체든 닿으면 이름과 태그를 출력
            Debug.Log("포탈 접촉 오브젝트: " + other.name + " | 태그: " + other.tag);

            if (other.CompareTag("MainCamera") || other.CompareTag("Player"))
            {
                Debug.Log("텔레포트 조건 일치! 이동을 시도합니다.");
                // 이동 로직...
            }

            // 2. 심리스 텔레포트 실행
            // XR Origin의 위치를 옮기되, 현재 카메라의 오프셋을 계산하여 보정
            Vector3 offset = playerTarget.position - playerOrigin.position;
            playerOrigin.position = portalExit.position - offset;

            // 이동 방향 정렬 (출구의 방향으로 플레이어를 회전)
            float angleDiff = Vector3.SignedAngle(transform.forward, portalExit.forward, Vector3.up);
            playerOrigin.Rotate(0, angleDiff + 180, 0);

            Debug.Log("심리스 이동 완료");
        }
    }
}