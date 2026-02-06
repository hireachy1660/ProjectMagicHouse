using UnityEngine;
using Oculus.Interaction; // 메타 인터랙션 네임스페이스 추가

public class VerticalRecursion : MonoBehaviour
{
    public Transform giantSpawnPoint;
    public float scaleMultiplier = 10f;

    private void OnTriggerEnter(Collider other)
    {
        // 태그 대신 Grabbable 컴포넌트가 있는지 확인
        if (other.GetComponentInParent<Grabbable>() != null)
        {
            SpawnGiantObject(other.gameObject);
        }
    }

    void SpawnGiantObject(GameObject smallObj)
    {
        // 1. 소환 위치 계산 (아까와 동일)
        Vector3 relativePos = smallObj.transform.position - transform.position;
        Vector3 giantSpawnPos = giantSpawnPoint.position + (relativePos * scaleMultiplier);
        giantSpawnPos.y += 10f; // 천장에서 떨어지게

        // 2. 거대 물건 소환
        GameObject giantObj = Instantiate(smallObj, giantSpawnPos, smallObj.transform.rotation);
        giantObj.transform.localScale = smallObj.transform.localScale * scaleMultiplier;

        // 3. 중요: 소환된 거대 물건은 잡을 수 없게 만들거나, 
        // 다시 트리거에 걸리지 않도록 Grabbable 컴포넌트 비활성화
        var grab = giantObj.GetComponentInChildren<Grabbable>();
        if (grab != null) grab.enabled = false;
    }
}