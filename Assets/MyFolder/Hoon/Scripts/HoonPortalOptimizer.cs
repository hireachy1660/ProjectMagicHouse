using UnityEngine;
using VRPortalToolkit.Rendering; // 에셋의 렌더러 네임스페이스

public class HoonPortalOptimizer : MonoBehaviour
{
    [Header("참조")]
    public Transform playerCamera;      // XR Origin의 Main Camera
    public PortalRendererBase portalRenderer; // 포탈의 Renderer 컴포넌트

    [Header("설정")]
    public float activeDistance = 5f;   // 실시간 화면을 켤 거리 (5미터)
    public float disableDistance = 6f;  // 실시간 화면을 끌 거리 (히스테리시스 적용)

    private bool _isRealtime = true;

    void Start()
    {
        // 자동으로 컴포넌트를 찾습니다.
        if (portalRenderer == null)
            portalRenderer = GetComponentInChildren<PortalRendererBase>();

        // 플레이어 카메라를 못 찾았다면 메인 카메라로 설정
        if (playerCamera == null && Camera.main != null)
            playerCamera = Camera.main.transform;
    }

    void Update()
    {
        if (playerCamera == null || portalRenderer == null) return;

        float distance = Vector3.Distance(transform.position, playerCamera.position);

        // 거리 기반 켜고 끄기 로직 (깜빡임 방지를 위해 켜는 거리와 끄는 거리에 차이를 둠)
        if (_isRealtime && distance > disableDistance)
        {
            SetPortalState(false);
        }
        else if (!_isRealtime && distance < activeDistance)
        {
            SetPortalState(true);
        }
    }

    void SetPortalState(bool active)
    {
        _isRealtime = active;

        // 포탈 렌더러 자체를 끄면 해당 포탈에 할당된 카메라 연산이 멈춥니다.
        portalRenderer.enabled = active;

        // (선택 사항) 꺼졌을 때 일반 사진 텍스처를 보여주는 코드를 여기에 추가할 수 있습니다.
        Debug.Log($"{gameObject.name} 포탈 실시간 렌더링: {active}");
    }
}