using UnityEngine;
using System.Collections;

public class Teleporter : MonoBehaviour
{
    public Transform receiver;
    public Transform playerRig;
    public Transform mainCamera;

    [Header("Settings")]
    public float exitOffset = 0.2f;  // 전송 후 나타날 지점
    public float threshold = 0.15f; // 이 거리보다 멀어지면 '빠져나왔다'고 판단 (Lock 해제)
    public float cooldownTime = 0.5f;

    private bool playerIsOverlapping = false;
    private bool isLocked = false;
    private bool inCooldown = false;
    private float lastLocalZ = 0f;

    void Update()
    {
        if (playerIsOverlapping && !inCooldown && mainCamera != null)
        {
            // 포탈의 중심점을 기준으로 카메라의 상대적 위치 계산
            Vector3 localCamPos = transform.InverseTransformPoint(mainCamera.position);

            if (isLocked)
            {
                // [의도 판별 1: 빠져나오기] 
                // 전송되어 온 후, 포탈 면에서 충분히 멀어지면 락을 해제합니다.
                if (Mathf.Abs(localCamPos.z) > threshold)
                {
                    isLocked = false;
                    Debug.Log($"<color=cyan><b>[Lock 해제]</b> {gameObject.name} - 이제 다시 통과 가능</color>");
                }
            }
            else
            {
                // [의도 판별 2: 통과하기 또는 되돌아가기]
                // 로컬 Z값의 부호가 바뀌었다는 것은 '면'을 가로질렀다는 뜻입니다.
                bool crossed = Mathf.Sign(lastLocalZ) != Mathf.Sign(localCamPos.z);

                if (crossed && Mathf.Abs(lastLocalZ) < 0.5f)
                {
                    ExecuteTeleport(localCamPos.z); // 현재 Z값을 전달
                    return;
                }
            }

            lastLocalZ = localCamPos.z;
        }
    }

    void ExecuteTeleport(float currentZ)
    {
        if (receiver == null || playerRig == null) return;

        // 1. 회전 계산 (상대적 회전 + 180도 반전으로 정면 응시)
        Quaternion halfTurn = Quaternion.Euler(0, 180f, 0);
        Quaternion relativeRot = receiver.rotation * halfTurn * Quaternion.Inverse(transform.rotation);

        // 2. 위치 계산
        Vector3 relativePos = transform.InverseTransformPoint(playerRig.position);
        relativePos = halfTurn * relativePos; // 위치도 180도 반전

        // 3. 물리적 이동
        // 상대방 포탈의 면 앞(exitOffset)으로 이동시킵니다.
        playerRig.position = receiver.TransformPoint(relativePos) + (receiver.forward * exitOffset);
        playerRig.rotation = relativeRot * playerRig.rotation;

        // 4. 출구 포탈(receiver) 설정
        var recScript = receiver.GetComponent<Teleporter>();
        if (recScript != null)
        {
            recScript.isLocked = true;          // 🔴 출구에 도착하자마자 락을 걸어 재전송 방지
            recScript.playerIsOverlapping = true;
            recScript.inCooldown = true;

            // 출구 입장에서 플레이어는 이미 면을 통과한 상태(Z가 exitOffset인 지점)로 설정
            recScript.lastLocalZ = exitOffset;
            recScript.StartCooldown();
        }

        // 5. 내 상태 초기화
        playerIsOverlapping = false;
        inCooldown = true;
        StartCooldown();

        Debug.Log($"<color=lime><b>[전송 완료]</b> {gameObject.name} -> {receiver.name}</color>");
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
        if (other.CompareTag("Player") || other.name.Contains("Anchor") || other.name.Contains("Camera"))
        {
            playerIsOverlapping = true;
            if (mainCamera != null)
                lastLocalZ = transform.InverseTransformPoint(mainCamera.position).z;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.name.Contains("Anchor") || other.name.Contains("Camera"))
        {
            playerIsOverlapping = false;
            isLocked = false; // 트리거를 아예 나가면 락은 무조건 해제
        }
    }
}