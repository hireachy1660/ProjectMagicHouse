using UnityEngine;

public class ModernPortal : MonoBehaviour
{
    [Header("Connections")]
    public ModernPortal targetPortal;
    public Transform playerRig;
    public Camera portalCamL, portalCamR;
    public MeshRenderer portalDisplay; // [중요] 포탈 화면이 나오는 Quad/Plane을 인스펙터에서 꼭 연결하세요
    public float activationDistance = 10f;

    private Transform mainCamTransform;
    private RenderTexture dynamicRTL, dynamicRTR;

    void Awake()
    {
        // 실시간 렌더 텍스처 생성
        dynamicRTL = new RenderTexture(1920, 1080, 24);
        dynamicRTR = new RenderTexture(1920, 1080, 24);

        if (portalCamL) portalCamL.targetTexture = dynamicRTL;
        if (portalCamR) portalCamR.targetTexture = dynamicRTR;
    }

    void Start()
    {
        mainCamTransform = Camera.main.transform;
        if (portalCamL) portalCamL.enabled = false;
        if (portalCamR) portalCamR.enabled = false;
    }

    // [이 함수가 없어서 에러가 났던 것입니다!]
    public void Link(ModernPortal target, Transform rig)
    {
        targetPortal = target; // 내 카메라가 이동해서 찍을 위치 (상대방 포탈)
        playerRig = rig;

        if (portalDisplay != null)
        {
            Material portalMat = portalDisplay.material;

            // [수정 핵심] 
            // 내 포탈 화면(Display)에는 '내 카메라(this)'가 상대방 쪽에서 찍고 있는 화면을 보여줘야 합니다.
            // target.dynamicRTL이 아니라 본인의 dynamicRTL을 할당하세요.
            portalMat.SetTexture("_LeftTex", this.dynamicRTL);
            portalMat.SetTexture("_RightTex", this.dynamicRTR);
        }

        Debug.Log($"<color=orange><b>[Link]</b> {gameObject.name}의 화면에 본인 카메라 시점을 할당했습니다.</color>");
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
        if (pCam == null || targetPortal == null) return;

        // 1. 내 현재 눈 위치와 회전 가져오기
        Matrix4x4 eyeMatrix = Camera.main.GetStereoViewMatrix(eye).inverse;
        Vector3 eyeWorldPos = eyeMatrix.GetColumn(3);
        Quaternion eyeWorldRot = mainCamTransform.rotation;

        // 2. [핵심] 180도 반전 쿼터니언
        Quaternion halfTurn = Quaternion.Euler(0, 180, 0);

        // 3. 상대 좌표 계산 (입구 포탈 기준)
        Vector3 relativePos = transform.InverseTransformPoint(eyeWorldPos);
        Quaternion relativeRot = Quaternion.Inverse(transform.rotation) * eyeWorldRot;

        // 4. [수정] 상대 좌표와 회전을 180도 반전시킴
        // 이렇게 해야 카메라가 포탈의 '반대편 안쪽'으로 들어가서 바깥을 찍습니다.
        relativePos = halfTurn * relativePos;
        relativeRot = halfTurn * relativeRot;

        // 5. 출구 포탈 기준으로 좌표 변환
        Vector3 targetPos = targetPortal.transform.TransformPoint(relativePos);
        Quaternion targetRot = targetPortal.transform.rotation * relativeRot;

        // 6. 포탈 카메라에 적용
        pCam.transform.position = targetPos;
        pCam.transform.rotation = targetRot;

        // 7. 프로젝션 매트릭스 동기화 (원근감 유지)
        pCam.projectionMatrix = Camera.main.GetStereoProjectionMatrix(eye);
    }
}