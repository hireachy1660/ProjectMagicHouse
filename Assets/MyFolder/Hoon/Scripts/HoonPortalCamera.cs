using UnityEngine;

public class HoonPortalCamera : MonoBehaviour
{
    [Header("필수 설정")]
    public Transform playerCamera;   // 내 진짜 눈 (Main Camera)
    public Transform srcPortal;      // 이 카메라의 화면이 '보여질' 포탈 (입구)
    public Transform dstPortal;      // 이 카메라가 실제로 '위치할' 포탈 (출구)

    void LateUpdate()
    {
        if (playerCamera == null || srcPortal == null || dstPortal == null) return;

        // 1. 플레이어 카메라의 위치를 srcPortal(입구) 기준으로 상대 좌표로 변환
        Vector3 relativePos = srcPortal.InverseTransformPoint(playerCamera.position);

        // 2. 포탈의 특성상 반대편을 비춰야 하므로 X와 Z축을 반전
        relativePos = new Vector3(-relativePos.x, relativePos.y, -relativePos.z);

        // 3. dstPortal(출구) 기준으로 가상 카메라의 위치 결정
        transform.position = dstPortal.TransformPoint(relativePos);

        // 4. 회전값 계산: 플레이어가 보는 방향을 상대적으로 계산하여 반전 후 적용
        Vector3 relativeForward = srcPortal.InverseTransformDirection(playerCamera.forward);
        relativeForward = new Vector3(-relativeForward.x, relativeForward.y, -relativeForward.z);
        transform.forward = dstPortal.TransformDirection(relativeForward);
    }
}