using UnityEngine;

public class ModernPortal : MonoBehaviour
{
    [Header("Connections")]
    public ModernPortal targetPortal;
    public Transform playerRig;
    public Camera portalCamL, portalCamR;
    public float activationDistance = 10f;

    private Transform mainCamTransform;

    void Start()
    {
        mainCamTransform = Camera.main.transform;

        // 초기 설정: 카메라들은 꺼두기
        if (portalCamL) portalCamL.enabled = false;
        if (portalCamR) portalCamR.enabled = false;
    }

    void LateUpdate()
    {
        if (targetPortal == null || playerRig == null) return;

        float dist = Vector3.Distance(mainCamTransform.position, transform.position);

        if (dist < activationDistance)
        {
            portalCamL.enabled = true;
            portalCamR.enabled = true;

            SyncEyeCamera(portalCamL, Camera.StereoscopicEye.Left);
            SyncEyeCamera(portalCamR, Camera.StereoscopicEye.Right);
        }
        else
        {
            portalCamL.enabled = false;
            portalCamR.enabled = false;
        }
    }

    void SyncEyeCamera(Camera pCam, Camera.StereoscopicEye eye)
    {
        if (pCam == null) return;

        // 1. 실제 사용자의 눈 위치(월드 좌표) 가져오기
        // GetStereoViewMatrix의 역행렬 4열이 정확한 눈의 월드 위치입니다.
        Matrix4x4 eyeMatrix = Camera.main.GetStereoViewMatrix(eye).inverse;
        Vector3 eyeWorldPos = eyeMatrix.GetColumn(3);
        Quaternion eyeWorldRot = mainCamTransform.rotation; // VR 시선 회전

        // 2. 입구 포탈(A) 기준 플레이어 눈의 상대적 위치/회전 계산
        Vector3 relativePos = transform.InverseTransformPoint(eyeWorldPos);
        Quaternion relativeRot = Quaternion.Inverse(transform.rotation) * eyeWorldRot;

        // 3. 출구 포탈(B)에서의 카메라 배치
        // 상대 좌표를 타겟 포탈의 월드 좌표로 변환
        Vector3 targetPos = targetPortal.transform.TransformPoint(relativePos);
        Quaternion targetRot = targetPortal.transform.rotation * relativeRot;

        // 4. 최종 적용
        pCam.transform.position = targetPos;
        pCam.transform.rotation = targetRot;

        // 5. VR 투영 행렬 동기화 (시차 유지의 핵심)
        pCam.projectionMatrix = Camera.main.GetStereoProjectionMatrix(eye);
    }
}