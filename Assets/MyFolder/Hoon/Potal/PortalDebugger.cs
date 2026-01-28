using UnityEngine;
using VRPortalToolkit.Portables;

public class PortalDebugger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // 1. 무엇과 충돌했는지 이름 출력
        Debug.Log($"<color=cyan>[Portal Contact]</color> {other.name} 물체가 포탈 영역에 들어왔습니다.");

        // 2. Rigidbody 체크 (PortalTransition이 인식하기 위한 필수 조건)
        if (other.attachedRigidbody == null)
        {
            Debug.LogWarning($"<color=yellow>[Warning]</color> {other.name}에 Rigidbody가 없습니다! 포탈이 이 물체를 무시할 수 있습니다.");
        }

        // 3. IPortable 인터페이스 체크 (실제 이동 가능 여부)
        var portable = other.GetComponentInParent<IPortable>();
        if (portable != null)
        {
            Debug.Log($"<color=green>[OK]</color> {other.name}은(는) 이동 가능한(IPortable) 물체입니다. (Origin: {portable.GetOrigin()})");
        }
        else
        {
            Debug.LogError($"<color=red>[Error]</color> {other.name}에 Portable 스크립트가 없어서 텔레포트 대상에서 제외됩니다.");
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // 카메라가 면을 넘고 있는지 실시간 좌표 체크
        var portable = other.GetComponentInParent<IPortable>();
        if (portable != null)
        {
            // 포탈의 로컬 Z축 기준으로 앞(+)/뒤(-) 판정
            Vector3 localPos = transform.InverseTransformPoint(portable.GetOrigin());
            // Debug.Log($"{other.name}의 상대 위치 Z: {localPos.z}"); // 너무 많이 찍힐 수 있어 주석처리

            if (localPos.z < 0)
            {
                Debug.Log("<color=magenta>[Pass Through]</color> 물체가 포탈 면을 통과(Z < 0)했습니다! 이동이 실행되어야 합니다.");
            }
        }
    }
}