using UnityEngine;

public class HoonPortalEntry : MonoBehaviour
{
    public GameObject insideObject;
    public Material normalMaterial;

    void OnTriggerEnter(Collider other)
    {
        // 태그 검사 없이, 무엇인가(카메라 포함) 닿으면 즉시 실행
        Debug.Log(other.name + "이(가) 통과함!"); // 콘솔창에 이름이 뜨는지 확인용

        Renderer renderer = insideObject.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = normalMaterial;
        }

        // 추가: 큐브가 여전히 안 보일 경우를 대비해 레이어도 Outside로 강제 변경
        insideObject.layer = LayerMask.NameToLayer("Outside");
    }
}


//using UnityEngine;

//public class PortalEntry : MonoBehaviour
//{
//    // 창문 안에서만 보이던 큐브(또는 그 큐브들을 담은 부모 오브젝트)
//    public GameObject insideObject;

//    // 일반적인 셰이더가 적용된 매트리얼 (1단계에서 만든 것)
//    public Material normalMaterial;

//    void OnTriggerEnter(Collider other)
//    {
//        // 플레이어가 창문을 통과하면
//        if (other.CompareTag("Player") || other.CompareTag("MainCamera"))
//        {
//            // 1. 큐브의 매트리얼을 일반 매트리얼로 교체 (이제 어디서든 보임)
//            Renderer renderer = insideObject.GetComponent<Renderer>();
//            if (renderer != null)
//            {
//                renderer.material = normalMaterial;
//            }

//            // 2. 만약 큐브가 여러 개라면, 부모를 지정해놓고 아래처럼 할 수도 있습니다.
//            /*
//            Renderer[] childRenderers = insideObject.GetComponentsInChildren<Renderer>();
//            foreach (Renderer r in childRenderers) {
//                r.material = normalMaterial;
//            }
//            */

//            Debug.Log("세계 진입: 이제 모든 각도에서 물체가 보입니다.");
//        }
//    }
//}