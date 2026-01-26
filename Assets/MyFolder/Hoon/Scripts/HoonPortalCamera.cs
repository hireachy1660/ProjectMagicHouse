using UnityEngine;
using UnityEngine.Rendering;

public class HoonVRPortalCameraUnified : MonoBehaviour
{
    [Header("포탈 설정")]
    public Transform srcPortal; // 입구
    public Transform dstPortal; // 출구

    [Header("플레이어 참조")]
    public Camera mainPlayerCamera; // CenterEyeAnchor의 카메라

    private Camera portalCam;

    void Awake()
    {
        portalCam = GetComponent<Camera>();
        // VR에서 양쪽 눈을 모두 렌더링하도록 설정
        portalCam.stereoTargetEye = StereoTargetEyeMask.Both;
    }

    void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
    }

    void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
    }

    // 1. 물리적 위치 이동: 렌더링과 상관없이 매 프레임 위치를 동기화하여 조이스틱 렉 방지
    void LateUpdate()
    {
        if (mainPlayerCamera == null || srcPortal == null || dstPortal == null) return;

        // [핵심 수정] 위치는 출구로 옮기되, 회전은 '절대' 건드리지 않습니다.
        // 가상 카메라의 몸체는 항상 월드 좌표의 기본(0,0,0) 회전을 유지하게 둡니다.
        portalCam.transform.position = dstPortal.position;
        portalCam.transform.rotation = Quaternion.identity; // 회전 고정!

        // 기본 속성 복사
        portalCam.aspect = mainPlayerCamera.aspect;
        portalCam.fieldOfView = mainPlayerCamera.fieldOfView;
        portalCam.nearClipPlane = mainPlayerCamera.nearClipPlane;
        portalCam.farClipPlane = mainPlayerCamera.farClipPlane;
    }

    void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (camera != portalCam) return;

        // 1. 포탈 변환 행렬 (위치와 회전만 사용)
        Matrix4x4 m_src = Matrix4x4.TRS(srcPortal.position, srcPortal.rotation, Vector3.one);
        Matrix4x4 m_dst = Matrix4x4.TRS(dstPortal.position, dstPortal.rotation, Vector3.one);

        // 입구에서 나가는 방향(180도)을 고려한 변환 행렬
        Matrix4x4 portalMatrix = m_dst * Matrix4x4.Rotate(Quaternion.Euler(0, 180, 0)) * m_src.inverse;

        // 2. 플레이어의 눈 행렬 주입 (여기서 모든 시야 회전이 결정됨)
        // 몸체(Transform)가 회전하지 않으므로, 이 행렬값이 곧 순수한 시야가 됩니다.
        portalCam.SetStereoViewMatrix(Camera.StereoscopicEye.Left, mainPlayerCamera.GetStereoViewMatrix(Camera.StereoscopicEye.Left) * portalMatrix.inverse);
        portalCam.SetStereoViewMatrix(Camera.StereoscopicEye.Right, mainPlayerCamera.GetStereoViewMatrix(Camera.StereoscopicEye.Right) * portalMatrix.inverse);

        portalCam.SetStereoProjectionMatrix(Camera.StereoscopicEye.Left, mainPlayerCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left));
        portalCam.SetStereoProjectionMatrix(Camera.StereoscopicEye.Right, mainPlayerCamera.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right));
    }
}