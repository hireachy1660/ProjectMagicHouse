using UnityEngine;
using Photon.Pun;

public class VRPlayerFallHandler : MonoBehaviourPunCallbacks // MonoBehaviourPun 대신 사용 권장
{
    [Header("Fall Settings")]
    public float deadZoneY = -10f;
    private Vector3 lastSafePosition;

    [Header("VR Rig References")]
    public Transform cameraRig;
    public Transform centerEyeAnchor;

    [Header("Check Settings")]
    public float checkInterval = 0.5f;
    private float timer = 0f;

    // PhotonView를 안전하게 찾기 위한 변수
    private PhotonView pv;

    private void Awake()
    {
        // 1. 현재 오브젝트 혹은 자식/부모에게서 PhotonView를 찾습니다.
        pv = GetComponent<PhotonView>();
        if (pv == null) pv = GetComponentInParent<PhotonView>();
        if (pv == null) pv = GetComponentInChildren<PhotonView>();
    }

    private void Start()
    {
        if (cameraRig == null) cameraRig = transform;
        if (centerEyeAnchor == null) centerEyeAnchor = Camera.main.transform;

        lastSafePosition = cameraRig.position;
    }

    private void Update()
    {
        // 2. pv가 아예 없는 경우를 대비해 Null 체크를 추가합니다. [cite: 2025-12-19]
        if (pv == null) return;

        // 3. 내 캐릭터가 아닐 경우 실행 안함 [cite: 2025-12-24]
        if (!pv.IsMine) return;

        UpdateSafePosition();

        if (centerEyeAnchor.position.y < deadZoneY) //[cite: 2025 - 12 - 19]
        {
            Respawn();
        }
    }

    private void UpdateSafePosition()
    {
        timer += Time.deltaTime;
        if (timer >= checkInterval)
        {
            timer = 0f;

            // 카메라 위치 아래에 바닥이 있는지 확인 [cite: 2025-12-19]
            if (Physics.Raycast(centerEyeAnchor.position + Vector3.up * 0.5f, Vector3.down, 2f))
            {
                lastSafePosition = cameraRig.position;
            }
        }
    }

    public void Respawn()
    {
        Debug.Log("<color=yellow>VR Rig 리스폰 실행</color>");

        CharacterController cc = cameraRig.GetComponentInChildren<CharacterController>();
        if (cc != null) cc.enabled = false;

        cameraRig.position = lastSafePosition;

        if (cc != null) cc.enabled = true;

        Rigidbody rb = cameraRig.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}