using UnityEngine;
using System.Collections;

public class Teleporter : MonoBehaviour
{
    public Transform receiver;
    public Transform playerRig;
    public Transform mainCamera;
    public Renderer playerRenderer;

    [Header("Settings")]
    public float exitOffset = -0.01f;
    public float cooldownTime = 0.5f; // [추가] 텔레포트 후 재발동 방지 시간

    private bool playerIsOverlapping = false;
    private bool isLocked = false;
    private bool inCooldown = false; // [추가] 쿨타임 체크
    private float lastLocalZ = 0f;

    void Update()
    {
        // 쿨타임 중이면 로직 건너뜀
        if (playerIsOverlapping && !inCooldown)
        {
            Vector3 localCamPos = transform.InverseTransformPoint(mainCamera.position);

            if (isLocked)
            {
                if (Mathf.Abs(localCamPos.z) > exitOffset * 0.8f)
                {
                    isLocked = false;
                    Debug.Log($"<color=cyan><b>[Lock 해제]</b> {gameObject.name}</color>");
                }
            }

            if (!isLocked)
            {
                if (Mathf.Sign(lastLocalZ) != Mathf.Sign(localCamPos.z) && Mathf.Abs(lastLocalZ) < 0.5f)
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

        Vector3 relativePos = transform.InverseTransformPoint(playerRig.position);
        Quaternion relativeRot = Quaternion.Inverse(transform.rotation) * playerRig.rotation;

        var recScript = receiver.GetComponent<Teleporter>();
        if (recScript != null)
        {
            recScript.playerIsOverlapping = true;
            recScript.isLocked = true;
            recScript.lastLocalZ = exitOffset;
            recScript.StartCooldown(); // [추가] 도착지 포탈에 쿨타임 부여
        }

        playerRig.position = receiver.TransformPoint(relativePos) + (receiver.forward * exitOffset);
        playerRig.rotation = receiver.rotation * relativeRot;

        playerIsOverlapping = false;

        if (playerRenderer != null)
            playerRenderer.material.SetVector("_PlanePosition", Vector3.up * -9999f);
    }

    public void StartCooldown()
    {
        StartCoroutine(CooldownRoutine());
    }

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
            Vector3 localCamPos = transform.InverseTransformPoint(mainCamera.position);

            // [개선] 진입 시에는 무조건 락을 걸지 않습니다. 
            // 락은 오직 '반대편에서 넘어왔을 때'만 걸려 있어야 합니다.
            if (!isLocked)
            {
                isLocked = false;
                Debug.Log($"<color=white><b>[진입 성공]</b> {gameObject.name}: 전송 대기 중 (Z:{localCamPos.z:F3})</color>");
            }
            else
            {
                Debug.Log($"<color=orange><b>[진입 유지]</b> {gameObject.name}: 이미 Lock 상태 (순간이동 도착 직후)</color>");
            }

            lastLocalZ = localCamPos.z;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.name.Contains("Anchor") || other.name.Contains("Camera"))
        {
            playerIsOverlapping = false;
            isLocked = false;
            Debug.Log($"<color=white><b>[퇴장]</b> {gameObject.name}: 상태 초기화</color>");

            if (playerRenderer != null)
                playerRenderer.material.SetVector("_PlanePosition", Vector3.up * -9999f);
        }
    }
}