using UnityEngine;

public class HoonPortalTeleport : MonoBehaviour
{
    public Transform player;          // VR 카메라 리그 또는 메인 카메라
    public Transform receiver;        // 가상 세계의 입구 (PortalExit)

    private bool playerIsOverlapping = false;

    void Update()
    {
        if (playerIsOverlapping)
        {
            // 플레이어가 포탈 면을 완전히 통과했을 때 실행
            // 단순히 좌표만 옮기는 방식:
            player.position = receiver.position;

            // 이동 후에는 현실 세계를 꺼버려도 됩니다.
            // GameObject.Find("OutsideObjects").SetActive(false); 

            playerIsOverlapping = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MainCamera") || other.CompareTag("Player"))
        {
            playerIsOverlapping = true;
        }
    }
}