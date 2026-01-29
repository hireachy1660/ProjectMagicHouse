using UnityEngine;

public class ModernPortal : MonoBehaviour
{
    [Header("Connections")]
    public ModernPortal targetPortal;
    public Transform playerRig;
    public Camera portalCamL, portalCamR;
    public float activationDistance = 10f; // 포탈 근처에서만 카메라 작동

    private Transform mainCamTransform;

    void Start()
    {
        mainCamTransform = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (targetPortal == null || playerRig == null) return;

        // 플레이어와 이 포탈 사이의 거리 체크
        float dist = Vector3.Distance(mainCamTransform.position, transform.position);

        // 플레이어가 이 포탈 앞에 있을 때만 '상대편' 카메라를 움직임
        if (dist < activationDistance)
        {
            SyncEyeCamera(portalCamL, Camera.StereoscopicEye.Left);
            SyncEyeCamera(portalCamR, Camera.StereoscopicEye.Right);

            // 상대편 카메라 활성화 (멀리 있을 땐 꺼두는 게 성능에 좋음)
            portalCamL.enabled = true;
            portalCamR.enabled = true;
        }
        else
        {
            // 내가 이 포탈 근처에 없으면 이 포탈이 관리하는 카메라는 끔
            portalCamL.enabled = false;
            portalCamR.enabled = false;
        }
    }

    void SyncEyeCamera(Camera pCam, Camera.StereoscopicEye eye)
    {




        Vector3 eyeWorldPos = Camera.main.GetStereoViewMatrix(eye).inverse.GetColumn(3);
        Quaternion eyeWorldRot = mainCamTransform.rotation;

        // A에서 B로 가는 벡터
        Vector3 portalOffset = targetPortal.transform.position - transform.position;

        // 카메라의 '월드 위치'를 내 눈 위치 + 오프셋으로 설정
        // pCam이 자식이라서 생기는 문제를 방지하기 위해 이렇게 대입 후 
        // 다시 한번 확인하는 로직이 유니티에선 필요할 수 있습니다.
        pCam.transform.position = eyeWorldPos + portalOffset;
        pCam.transform.rotation = eyeWorldRot;



        // 1. 플레이어 눈의 위치/회전
        Vector3 eyePos = Camera.main.GetStereoViewMatrix(eye).inverse.GetColumn(3);
        Quaternion eyeRot = mainCamTransform.rotation;

        // 2. 입구 포탈(A) 기준 플레이어의 상대 위치 계산
        // 뷰가 0,0,0이므로 플레이어는 포탈의 Z- 영역에 있습니다.
        Vector3 relativePos = transform.InverseTransformPoint(eyePos);

        // 회전: 입구 포탈의 기준 방향에서 내 시선이 얼마나 틀어졌는지 계산
        Quaternion relativeRot = Quaternion.Inverse(transform.rotation) * eyeRot;

        // 3. 출구 포탈(B)에서 카메라 배치
        // 사용자 의도: "내가 보는 방향 그대로!"
        // 입구와 출구가 이미 부모 회전(0 vs 180)으로 방향이 잡혀 있으므로, 
        // 상대 좌표를 그대로 더해주면 시선이 일치하게 됩니다.
        Vector3 targetPos = targetPortal.transform.TransformPoint(relativePos);
        Quaternion targetRot = targetPortal.transform.rotation * relativeRot;

        // 4. 최종 적용
        pCam.transform.position = targetPos;
        pCam.transform.rotation = targetRot;

        // 5. VR 투영 행렬 동기화

        pCam.projectionMatrix = Camera.main.GetStereoProjectionMatrix(eye);
    }
}