using UnityEngine;

public class HoonPortalTransition : MonoBehaviour
{
    public GameObject placeWorldRoot;    // 현실 세계의 부모 오브젝트 (Floor, Pillar 등)
    public GameObject place2WorldRoot; // 가상 세계의 부모 오브젝트
    public Material[] stencilReaderMats;  // 마우스로 여러 개를 넣을 수 있도록 배열([])로 선언합니다.

    void Start()
    {
        // 게임이 시작될 때 모든 매트리얼을 다시 '구멍 안에서만 보이기(Equal)' 상태로 초기화합니다.
        if (stencilReaderMats != null)
        {
            foreach (Material mat in stencilReaderMats)
            {
                if (mat != null)
                {
                    // 3번은 Equal(구멍 안에서만 보임)입니다.
                    mat.SetInt("_StencilComp", 3);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 플레이어가 태그가 "Player"인 경우에만 작동
        if (other.CompareTag("Player")|| other.CompareTag("MainCamera"))
        {
            EnterVirtualWorld();
        }
    }

    void EnterVirtualWorld()
    {
        // 1. 현실 세계를 끕니다.
        if (placeWorldRoot != null) placeWorldRoot.SetActive(false);

        // 2. 배열에 담긴 모든 매트리얼의 스텐실 조건을 Always(8)로 변경합니다.
        if (stencilReaderMats != null)
        {
            foreach (Material mat in stencilReaderMats)
            {
                if (mat != null)
                {
                    mat.SetInt("_StencilComp", (int)UnityEngine.Rendering.CompareFunction.Always);
                }
            }
        }

        Debug.Log("모든 매트리얼의 스텐실 제약이 해제되었습니다!");
    }
}