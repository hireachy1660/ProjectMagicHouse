using UnityEngine;
using UnityEngine.InputSystem;

public class VRMenuController : MonoBehaviour
{
    // 인스펙터에서 LeftHand / Menu 버튼 액션을 할당하세요.
    // 보통 XRI LeftHand/Menu 또는 PrimaryButton 등을 선택합니다.
    public InputActionProperty menuButton;

    void Update()
    {
        // 버튼이 눌렸는지 체크
        if (menuButton.action.WasPressedThisFrame())
        {
            // 세팅 매니저가 있는지 확인 (싱글톤)
            if (SettingManager.Instance == null) return;

            // 이미 켜져있으면 닫고, 꺼져있으면 내 손앞에 열기
            if (SettingManager.Instance.settingCanvas.activeSelf)
            {
                SettingManager.Instance.CloseSetting();
            }
            else
            {
                // 이 스크립트가 붙은 컨트롤러(this.transform)를 넘겨줌
                SettingManager.Instance.OpenSettingVR(this.transform);
            }
        }
    }
}