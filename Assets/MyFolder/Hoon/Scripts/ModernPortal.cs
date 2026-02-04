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
        dynamicRTL = new RenderTexture(1024, 1024, 24);
        dynamicRTR = new RenderTexture(1024, 1024, 24);

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
        targetPortal = target;
        playerRig = rig;

        if (portalDisplay != null)
        {
            Material portalMat = portalDisplay.material;
            // 쉐이더의 텍스처 변수 이름이 다를 경우 아래 이름을 수정하세요
            portalMat.SetTexture("_LeftTex", target.dynamicRTL);
            portalMat.SetTexture("_RightTex", target.dynamicRTR);
        }

        Debug.Log($"<color=orange><b>[Link]</b> {gameObject.name}가 {target.name}과 연결되었습니다.</color>");
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

        Matrix4x4 eyeMatrix = Camera.main.GetStereoViewMatrix(eye).inverse;
        Vector3 eyeWorldPos = eyeMatrix.GetColumn(3);
        Quaternion eyeWorldRot = mainCamTransform.rotation;

        Vector3 relativePos = transform.InverseTransformPoint(eyeWorldPos);
        Quaternion relativeRot = Quaternion.Inverse(transform.rotation) * eyeWorldRot;

        Vector3 targetPos = targetPortal.transform.TransformPoint(relativePos);
        Quaternion targetRot = targetPortal.transform.rotation * relativeRot;

        pCam.transform.position = targetPos;
        pCam.transform.rotation = targetRot;
        pCam.projectionMatrix = Camera.main.GetStereoProjectionMatrix(eye);
    }
}