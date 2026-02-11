using UnityEngine;
public class PlayerExitManager : MonoBehaviour
{
    public Transform realWorldExitPoint; // 현실 맵(House2) 밖의 탈출 좌표
    public VerticalRecursion vrScript;   // 문 설치 여부 확인용
    public bool isDoorOpen = false;      // 패스파인더가 Ray로 문을 열었을 때 true로 바뀜

    private void OnTriggerEnter(Collider other)
    {
        // 1. 문이 설치됨 + 2. 문이 열림 + 3. 대상이 패스파인더임
        if (vrScript != null && vrScript.isDoorSpawned && isDoorOpen)
        {
            if (other.CompareTag("Pathfinder"))
            {
                other.transform.localScale = Vector3.one; // 크기 복구
                other.transform.position = realWorldExitPoint.position; // 현실 밖으로 이동
            }
        }
    }
}