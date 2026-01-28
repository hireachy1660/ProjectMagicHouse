using UnityEngine;

public class HoonVRDualPortalCamera : MonoBehaviour
{
    public enum Eye { Left, Right }

    [Header("설정")]
    public Eye eyeSide; // 이 카메라가 왼쪽 눈인지 오른쪽 눈인지 설정
    public Transform eyeAnchor; // LeftEyeAnchor 또는 RightEyeAnchor 연결

    [Header("포탈 설정")]
    public Transform srcPortal; // 입구 포탈
    public Transform dstPortal; // 출구 포탈

    void LateUpdate()
    {
        if (eyeAnchor == null || srcPortal == null || dstPortal == null) return;

        // 1. 위치 계산: 실제 눈의 위치를 입구 기준 상대 좌표로 변환 후, 180도 회전하여 출구에 적용
        Vector3 relativePos = srcPortal.InverseTransformPoint(eyeAnchor.position);
        // 포탈 통과 시 반대편을 바라봐야 하므로 180도 회전 적용
        Vector3 portalOffsetPos = Quaternion.Euler(0, 180, 0) * relativePos;
        transform.position = dstPortal.TransformPoint(portalOffsetPos);

        // 2. 회전 계산: 입구와 출구 사이의 회전 차이를 계산하여 적용
        // 포탈의 기본 회전(180도 반전)을 포함한 회전 변량
        Quaternion portalRotDelta = dstPortal.rotation * Quaternion.Inverse(srcPortal.rotation * Quaternion.Euler(0, 180, 0));
        transform.rotation = portalRotDelta * eyeAnchor.rotation;
    }
}