using UnityEngine;

public class RoleGrabSync : GrabSync
{
    [Header("역할 데이터 시스템")]
    [SerializeField]
    private GameStatusSO gameStatus; // 인스펙터에서 연결할 SO
    [SerializeField]
    private bool IsDebugMode;

    protected override void Start()
    {
        // 1. 부모의 널 체크 실행
        base.Start();
        if (!IsDebugMode) return;
        // 2. 즉시 권한 확인 후 상호작용 잠금 결정
        ApplyRoleAuthorityLocally();
    }

    /// <summary>
    /// 로컬에서 내 역할과 오브젝트의 태그를 비교하여 상호작용 가능 여부를 결정합니다.
    /// </summary>
    public void ApplyRoleAuthorityLocally()
    {
        if (gameStatus == null)
        {
            Debug.LogError($"{gameObject.name}: GameStatusSO가 할당되지 않았습니다!");
            return;
        }

        // 내 역할(예: Detective)과 이 오브젝트의 태그(예: Detective)가 일치하지 않으면
        if (!gameObject.CompareTag(gameStatus.myRole))
        {
            LockInteraction();
        }
        else
        {
            UnlockInteraction();
        }
    }

    private void LockInteraction()
    {
        if (interactable != null)
        {
            // Meta Interaction SDK의 인터랙터블을 로컬에서 비활성화
            interactable.enabled = false;
            Debug.Log($"<color=red>[권한 잠금]</color> {gameObject.name}은 {gameStatus.myRole} 역할이 사용할 수 없습니다.");
        }
    }

    private void UnlockInteraction()
    {
        if (interactable != null)
        {
            interactable.Enable();
            Debug.Log($"<color=green>[권한 승인]</color> {gameObject.name} 상호작용 가능 (역할: {gameStatus.myRole})");
        }
    }

    // 부모의 이벤트는 이제 검증된 상태에서만 호출되도록 안전하게 오버라이드
    public override void OnGrabEvent()
    {
        // 이미 Start에서 Disable 처리를 했으므로, 이 이벤트가 호출되었다는 것은 권한이 있다는 뜻임
        base.OnGrabEvent();
    }
}