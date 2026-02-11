using UnityEngine;

public class PlayerScaleManager : MonoBehaviour
{
    public Transform miniHouseSpawnPoint; // 미니어처 집 안의 시작점
    public Transform realWorldSpawnPoint; // 현실 거실의 시작점
    public float miniScale = 0.1f;

    private void OnTriggerEnter(Collider other)
    {
        // 1. 패스파인더 처리
        if (other.CompareTag("Pathfinder"))
        {
            CharacterController cc = other.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false; // 위치 이동을 위해 잠시 끔

            other.transform.localScale = Vector3.one * miniScale;
            other.transform.position = miniHouseSpawnPoint.position;

            if (cc != null) cc.enabled = true; // 다시 켬
        }
        // 2. 인퀴지터 처리
        else if (other.CompareTag("Inquisitor"))
        {
            CharacterController cc = other.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            other.transform.position = realWorldSpawnPoint.position;
            other.transform.localScale = Vector3.one; // 원래 크기 유지

            if (cc != null) cc.enabled = true;
        }
    }
}