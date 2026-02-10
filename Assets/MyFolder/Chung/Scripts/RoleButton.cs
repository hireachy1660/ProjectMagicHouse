using UnityEngine;

public class RoleButton : MonoBehaviour
{
    // 인스펙터에서 드롭다운으로 역할을 고릅니다.
    [SerializeField] private Role role;
    //[SerializeField] private RoleSelector selector;

    public void OnClick()
    {
        // 내 버튼에 설정된 이넘 값을 매니저에 던져줍니다.
        //if (selector != null) selector.SelectRoleAndStart(role);
    }
}