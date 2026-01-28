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

        pCam.projectionMatrix = Camera.main.GetStereoProjectionMatrix(eye);
    }
}