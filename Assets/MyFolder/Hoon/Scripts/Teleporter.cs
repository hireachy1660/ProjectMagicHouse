using UnityEngine;
using System.Collections;

public class Teleporter : MonoBehaviour
{
    public Transform receiver;
    public Transform playerRig;
    public Transform mainCamera;
    public Renderer playerRenderer;

    [Header("Settings")]
    public float exitOffset = 0.05f; // 약간의 여유를 두어 끼임 방지
    public float cooldownTime = 0.5f;

    private bool playerIsOverlapping = false;
    private bool isLocked = false;
    private bool inCooldown = false;
    private float lastLocalZ = 0f;

    void Update()
    {
        if (playerIsOverlapping && !inCooldown)
        {
            Vector3 localCamPos = transform.InverseTransformPoint(mainCamera.position);

            if (isLocked)
            {
                // 락 해제 로직 (출구에서 충분히 떨어졌을 때)
                if (Mathf.Abs(localCamPos.z) > 0.1f)
                {
                    isLocked = false;
                    Debug.Log($"<color=cyan><b>[Lock 해제]</b> {gameObject.name}</color>");
                }
            }

            if (!isLocked)
            {
                // 면 통과 감지 또는 너무 가까워졌을 때 강제 전송(안전장치)
                bool crossed = Mathf.Sign(lastLocalZ) != Mathf.Sign(localCamPos.z);
                bool forced = localCamPos.z < -0.02f; // 살짝 뒤로 넘어갔을 때

                if ((crossed || forced) && Mathf.Abs(lastLocalZ) < 0.5f)
                {
                    ExecuteTeleport();
                    return;
                }
            }

            lastLocalZ = localCamPos.z;
            UpdateClippingProperties();
        }
    }

    void ExecuteTeleport()
    {
        if (receiver == null || playerRig == null) return;

        // 1. 회전 변환 계산 (핵심)
        // 입구와 출구는 서로 마주보고 있으므로 180도 회전을 더해줘야 정면이 맞습니다.
        Quaternion relativeRot = receiver.rotation * Quaternion.Inverse(transform.rotation);

        // 2. 위치 변환
        Vector3 relativePos = transform.InverseTransformPoint(playerRig.position);
        playerRig.position = receiver.TransformPoint(relativePos) + (receiver.forward * exitOffset);

        // 3. 회전 적용 (몸 전체를 돌려줌)
        playerRig.rotation = relativeRot * playerRig.rotation;

        // 4. 속도(Velocity) 변환 (Rigidbody가 있을 경우 조작감 유지의 핵심)
        Rigidbody rb = playerRig.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = relativeRot * rb.linearVelocity;
        }

        // 5. 상대방 포탈 상태 설정
        var recScript = receiver.GetComponent<Teleporter>();
        if (recScript != null)
        {
            recScript.playerIsOverlapping = true;
            recScript.isLocked = true;
            recScript.lastLocalZ = exitOffset;
            recScript.StartCooldown();
        }

        playerIsOverlapping = false;

        if (playerRenderer != null)
            playerRenderer.material.SetVector("_PlanePosition", Vector3.up * -9999f);

        Debug.Log($"<color=lime><b>[전송 완료]</b> {gameObject.name} -> {receiver.name}</color>");
    }

    public void StartCooldown() => StartCoroutine(CooldownRoutine());
    IEnumerator CooldownRoutine()
    {
        inCooldown = true;
        yield return new WaitForSeconds(cooldownTime);
        inCooldown = false;
    }

    void UpdateClippingProperties()
    {
        if (playerRenderer == null) return;
        playerRenderer.material.SetVector("_PlanePosition", transform.position);
        playerRenderer.material.SetVector("_PlaneNormal", transform.forward);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.name.Contains("Anchor") || other.name.Contains("Camera"))
        {
            playerIsOverlapping = true;
            lastLocalZ = transform.InverseTransformPoint(mainCamera.position).z;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.name.Contains("Anchor") || other.name.Contains("Camera"))
        {
            playerIsOverlapping = false;
            isLocked = false;
            if (playerRenderer != null)
                playerRenderer.material.SetVector("_PlanePosition", Vector3.up * -9999f);
        }
    }
}