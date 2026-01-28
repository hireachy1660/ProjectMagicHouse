using UnityEngine;
using static UnityEngine.Camera;

public class ModernPortal : MonoBehaviour
{
    [Header("Connections")]
    public ModernPortal targetPortal;
    public Transform playerRig;
    public Camera portalCamL, portalCamR;

    private Transform mainCamTransform;

    void Start()
    {
        // 빌딩블록의 센터 아이 카메라를 찾습니다.
        mainCamTransform = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (targetPortal == null || playerRig == null) return;

        // 에러 해결: 명시적으로 StereoscopicEye 타입을 사용합니다.
        SyncEyeCamera(portalCamL, Camera.StereoscopicEye.Left);
        SyncEyeCamera(portalCamR, Camera.StereoscopicEye.Right);
    }

    // 매개변수 타입을 Camera.StereoscopicEye로 변경합니다.
    void SyncEyeCamera(Camera pCam, Camera.StereoscopicEye eye)
    {
        // 1. 플레이어 눈의 위치와 회전 가져오기
        Vector3 eyePos = Camera.main.GetStereoViewMatrix(eye).inverse.GetColumn(3);
        Quaternion eyeRot = mainCamTransform.rotation;

        // 2. 입구 포탈(A) 기준 플레이어의 상대 위치/회전 계산
        // 월드 좌표를 포탈 A의 로컬 좌표로 변환
        Vector3 relativePos = transform.InverseTransformPoint(eyePos);
        Quaternion relativeRot = Quaternion.Inverse(transform.rotation) * eyeRot;

        // 3. 출구 포탈(B)에서 카메라 위치 잡기
        // 입구와 출구가 병렬이므로, 입구 뒤쪽(Z-)에서 보던 걸 출구 뒤쪽(Z-)에서 보게 함
        // 병렬 구조에서는 Z값을 반전시키지 않고 그대로 둡니다.
        Vector3 targetPos = targetPortal.transform.TransformPoint(relativePos);

        // 회전 역시 입구에서 보던 각도 그대로 출구에서 적용
        Quaternion targetRot = targetPortal.transform.rotation * relativeRot;

        // 4. 카메라에 적용
        pCam.transform.position = targetPos;
        pCam.transform.rotation = targetRot;

        // 5. VR 투영 행렬 동기화
        pCam.projectionMatrix = Camera.main.GetStereoProjectionMatrix(eye);
    }
}