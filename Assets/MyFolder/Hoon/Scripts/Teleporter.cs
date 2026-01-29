using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [Header("Portal Settings")]
    public Transform receiver;      // 목적지 포탈
    public Transform playerRig;     // 플레이어 리드(OVRRig 등)
    public Transform mainCamera;    // 메인 카메라 (머리)
    public Renderer playerRenderer; // 클리핑을 적용할 플레이어 메쉬

    private bool playerIsOverlapping = false;
    private bool isTeleportingJustNow = false;

    void Update()
    {
        if (playerIsOverlapping)
        {
            // 1. 포탈 면 기준 카메라의 로컬 위치 계산
            Vector3 localCamPos = transform.InverseTransformPoint(mainCamera.position);

            // 2. 면 뒤(Z <= 0)로 넘어갔는지 판정
            bool isBehindPlane = localCamPos.z <= 0f;

            // 3. 텔레포트 실행 (방금 이동해온 게 아닐 때만)
            if (!isTeleportingJustNow && isBehindPlane)
            {
                ExecuteTeleport();
            }
            // 4. 면 앞으로 고개를 빼면 다시 이동 준비 상태로 리셋
            else if (!isBehindPlane)
            {
                isTeleportingJustNow = false;
            }

            // 셰이더 클리핑 업데이트 (고개 들이밀 때 몸 자르기)
            UpdateClippingProperties();
        }
    }

    void ExecuteTeleport()
    {
        if (receiver == null || playerRig == null) return;

        // [Portal.cs 스타일의 상대 좌표 계산]
        // 1. 현재 포탈 기준의 상대적 위치와 회전을 계산
        Vector3 relativePos = transform.InverseTransformPoint(playerRig.position);
        Quaternion relativeRot = Quaternion.Inverse(transform.rotation) * playerRig.rotation;

        // 2. 목적지 포탈의 스크립트 참조
        var recScript = receiver.GetComponent<Teleporter>();
        if (recScript != null)
        {
            recScript.playerIsOverlapping = true;
            recScript.isTeleportingJustNow = true; // 무한 루프(밀어냄) 방지
        }

        // 3. 목적지 위치로 이동 (상대 좌표를 목적지 월드 좌표로 변환)
        // 아주 미세한 오차로 인한 밀어냄을 방지하기 위해 forward 방향으로 0.01m 여유를 둡니다.
        playerRig.position = receiver.TransformPoint(relativePos) + (receiver.forward * 0.01f);
        playerRig.rotation = receiver.rotation * relativeRot;

        playerIsOverlapping = false;
        Debug.Log("<color=lime>현대식 VR 텔레포트 성공!</color>");
    }

    void UpdateClippingProperties()
    {
        if (playerRenderer == null) return;

        // 셰이더에게 현재 포탈 면의 위치와 방향을 전달
        // 셰이더는 이 Plane 기준 뒤쪽 메쉬를 투명하게 지워 '공간 전이' 낭만을 만듭니다.
        playerRenderer.material.SetVector("_PlanePosition", transform.position);
        playerRenderer.material.SetVector("_PlaneNormal", transform.forward);
    }

    private void OnTriggerEnter(Collider other)
    {
        // 카메라(머리)나 플레이어 몸체가 들어오면 오버랩 시작
        if (other.CompareTag("Player") || other.name.Contains("Camera"))
        {
            playerIsOverlapping = true;

            // 진입 시점에 이미 면 뒤에 있다면 (방금 이동해온 경우) 보호막 작동
            Vector3 localCamPos = transform.InverseTransformPoint(mainCamera.position);
            isTeleportingJustNow = localCamPos.z <= 0f;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.name.Contains("Camera"))
        {
            playerIsOverlapping = false;
            isTeleportingJustNow = false;

            // 포탈을 완전히 벗어나면 몸이 다시 온전히 보이도록 클리핑 초기화
            if (playerRenderer != null)
                playerRenderer.material.SetVector("_PlanePosition", new Vector3(0, -9999, 0));
        }
    }
}