using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Realtime;

public class RoomItem : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI roomInfoText;    // 방이름과 인원수 표시
    public Button JoinButton;               // 입장 버튼

    private string roomName;
    private LoginManager manager;

    // LoginManager에서 이 함수를 호출하여 방 정보를 셋팅함
    public void SetInfo(RoomInfo info, LoginManager loginManager)
    {
        manager = loginManager;
        roomName = info.Name;

        // 텍스트 업데이트 예 : "비밀의 방 (1/2)"
        roomInfoText.text = $"{info.Name} ({info.PlayerCount}/{info.MaxPlayers})";

        // 입장 버튼 클릭 시 입장 함수 호출
        JoinButton.onClick.RemoveAllListeners();    // 중복 방지
        JoinButton.onClick.AddListener(onClickRoom);
    
    }

    // 버튼 클릭 시 호출될 함수
    public void onClickRoom()
    {
        Debug.Log($"{roomName} 방에 입장을 시도합니다.");
        manager.JoinRoom(roomName); // Login매니저를 통해 방 입장 실행
    }
}