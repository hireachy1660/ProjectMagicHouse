using UnityEngine;
using System.Collections;
using Photon.Pun; // 추가 [cite: 2025-12-19]

public class Teleporter : MonoBehaviourPun // 상속 변경 [cite: 2025-12-24]
{
    public Transform receiver;
    public Transform playerRig;
    public Transform mainCamera;

    [Header("Settings")]
    public float exitOffset = 0.2f;
    public float threshold = 0.15f;
    public float cooldownTime = 0.5f;

    private bool playerIsOverlapping = false;
    private bool isLocked = false;
    private bool inCooldown = false;
    private float lastLocalZ = 0f;

    void Update()
    {
        // 업데이트 로직은 로컬 플레이어가 중첩되었을 때만 실행됩니다.
        if (playerIsOverlapping && !inCooldown && mainCamera != null)
        {
            Vector3 localCamPos = transform.InverseTransformPoint(mainCamera.position);

            if (isLocked)
            {
                if (Mathf.Abs(localCamPos.z) > threshold)
                {
                    isLocked = false;
                }
            }
            else
            {
                bool crossed = Mathf.Sign(lastLocalZ) != Mathf.Sign(localCamPos.z);
                if (crossed && Mathf.Abs(lastLocalZ) < 0.5f)
                {
                    ExecuteTeleport(localCamPos.z);
                    return;
                }
            }
            lastLocalZ = localCamPos.z;
        }
    }

    void ExecuteTeleport(float currentZ)
    {
        if (receiver == null || playerRig == null) return;

        // 이동은 각자의 클라이언트에서 본인의 캐릭터만 옮깁니다. [cite: 2025-12-19]
        Quaternion halfTurn = Quaternion.Euler(0, 180f, 0);
        Quaternion relativeRot = receiver.rotation * halfTurn * Quaternion.Inverse(transform.rotation);

        Vector3 relativePos = transform.InverseTransformPoint(playerRig.position);
        relativePos = halfTurn * relativePos;

        playerRig.position = receiver.TransformPoint(relativePos) + (receiver.forward * exitOffset);
        playerRig.rotation = relativeRot * playerRig.rotation;

        var recScript = receiver.GetComponent<Teleporter>();
        if (recScript != null)
        {
            recScript.isLocked = true;
            recScript.playerIsOverlapping = true;
            recScript.inCooldown = true;
            recScript.lastLocalZ = exitOffset;
            recScript.StartCooldown();
        }

        playerIsOverlapping = false;
        inCooldown = true;
        StartCooldown();
    }

    public void StartCooldown() => StartCoroutine(CooldownRoutine());
    IEnumerator CooldownRoutine()
    {
        inCooldown = true;
        yield return new WaitForSeconds(cooldownTime);
        inCooldown = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        // [네트워크 핵심] 충돌한 물체가 내(Local Player) 것인지 확인합니다. [cite: 2025-12-19]
        PhotonView pv = other.GetComponent<PhotonView>();
        if (pv != null && pv.IsMine)
        {
            // 플레이어 태그나 특정 레이어 확인 로직 병행 권장
            playerIsOverlapping = true;
            if (mainCamera != null)
                lastLocalZ = transform.InverseTransformPoint(mainCamera.position).z;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PhotonView pv = other.GetComponent<PhotonView>();
        if (pv != null && pv.IsMine)
        {
            playerIsOverlapping = false;
            isLocked = false;
        }
    }
}