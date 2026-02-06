using UnityEngine;

public class MaterialChanger : MonoBehaviour
{
    public Material targetMaterial;

    // 플레이 버튼(▶)을 누르면 즉시 실행됩니다.
    void Awake()
    {
        if (targetMaterial == null)
        {
            Debug.LogError("타겟 매티리얼(M_StencilReader)이 비어있습니다!");
            return;
        }

        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer ren in renderers)
        {
            ren.material = targetMaterial;
        }

        Debug.Log("<color=yellow>" + renderers.Length + "개의 자식 오브젝트 매티리얼 변경 성공!</color>");
    }
}