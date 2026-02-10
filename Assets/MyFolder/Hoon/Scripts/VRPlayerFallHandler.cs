using UnityEngine;

public class VRPlayerFallHandler : MonoBehaviour
{
    [Header("Fall Settings")]
    public float deadZoneY = -10f; // 이 높이보다 낮아지면 리스폰

    [Header("Respawn Target")]
    [Tooltip("추락 시 이동할 안전한 위치(Transform)를 연결하세요")]
    public Transform respawnPoint;

    [Header("VR Rig References")]
    [Tooltip("Building Blocks 최상위 부모 (Camera Rig)")]
    public Transform cameraRig;

    [Tooltip("높이 체크용 CenterEyeAnchor")]
    public Transform centerEyeAnchor;

    private void Start()
    {
        // 리퍼런스 자동 할당
        if (cameraRig == null) cameraRig = transform;
        if (centerEyeAnchor == null) centerEyeAnchor = Camera.main.transform;

        // 리스폰 포인트가 비어있다면 시작 위치를 리스폰 포인트로 임시 저장
        if (respawnPoint == null)
        {
            GameObject tempPoint = new GameObject("Default_Respawn_Point");
            tempPoint.transform.position = cameraRig.position;
            respawnPoint = tempPoint.transform;
            Debug.LogWarning("[FallHandler] 리스폰 포인트가 설정되지 않아 시작 위치로 설정되었습니다.");
        }
    }

    private void Update()
    {
        // 복잡한 바닥 검사 없이 Y값만 단순 체크 (최적화 최상)
        if (centerEyeAnchor.position.y < deadZoneY)
        {
            Respawn();
        }
    }

    public void Respawn()
    {
        Debug.Log($"<color=cyan>[FallHandler] 지정된 위치({respawnPoint.position})로 리스폰합니다.</color>");

        // 1. CharacterController 비활성화 (순간이동 시 필수)
        CharacterController cc = cameraRig.GetComponentInChildren<CharacterController>();
        if (cc != null) cc.enabled = false;

        // 2. 최상위 Rig 전체를 리스폰 포인트의 위치와 회전으로 이동
        cameraRig.position = respawnPoint.position;
        cameraRig.rotation = respawnPoint.rotation;

        // 3. 물리 속도 초기화
        Rigidbody rb = cameraRig.GetComponent<Rigidbody>();
        if (rb == null) rb = cameraRig.GetComponentInChildren<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // 4. CharacterController 재활성화
        if (cc != null) Invoke("EnableCC", 0.1f);
    }

    private void EnableCC()
    {
        CharacterController cc = cameraRig.GetComponentInChildren<CharacterController>();
        if (cc != null) cc.enabled = true;
    }
}