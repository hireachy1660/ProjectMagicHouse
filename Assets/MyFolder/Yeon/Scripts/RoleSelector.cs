using ExitGames.Client.Photon;
using Photon.Pun;
using TMPro;
using UnityEngine;
using WebSocketSharp;

public class RoleSelector : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private GameStatusSO gameStatus;
    [SerializeField] 
    private TextMeshProUGUI InfoText;

    // 버튼에 이 함수 연결 ( 인자 값으로 프리팹 이름을 직접 쓴다 )
    //####################수정된 코드#######################
    // 현재 로직은 버튼에 달린 스크립트로 부터 이넘 값을 받아 이넘 값을 문자열로 변환해 검사
    public void SelectRoleAndStart(Role _myRole)
    {
        InfoText.text = "Role Select"; // 이전 메시지 초기화

        string targetRole = _myRole.ToString();

        // 다른 플레이어 중 같은 역할을 가진 사람이 있는지 검사
        foreach (var player in PhotonNetwork.PlayerListOthers)
        {
            if (player.CustomProperties.TryGetValue("MyRole", out object role))
            {
                if ((string)role == targetRole)
                {
                    Debug.LogWarning("이미 다른 플레이어가 선택한 역할입니다!");
                    // 여기서 유저에게 알림 UI를 띄우면 더 좋습니다.
                    InfoText.text = "이미 선택된 역할 입니다";
                    return;
                }
            }
        }

        // 중복이 없을 때만 내 가방에 저장
        Hashtable props = new Hashtable { { "MyRole", targetRole } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        gameStatus.myRole = targetRole;
        Debug.Log(targetRole + " 선택 완료!");
    }
}
   // public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, Hashtable changedProps)
   //{
   //     // 내 LocalPlayer 가방이 바뀌었고, 그 안에 MyRole이 있다면 씬 이동한다.
   //     if(targetPlayer.IsLocal && changedProps.ContainsKey("MyRole"))
   //     {
   //         Debug.Log("서버 확인 완료! 이제 씬을 이동한다.");
   //         if(PhotonNetwork.IsMasterClient)
   //         {
   //             PhotonNetwork.LoadLevel("GameScene");
   //         }
   //     }
   // }   


//        // 씬이동 ( 방장이 아니어도 테스트 가능하게 일단 IsMasterClient 체크 제외하고
//        // 나중에 2인 플레이가 확실해지면 다시 if(PhotonNetwork.IsMaterClient)를 넣자
//(PhotonNetwork.IsMasterClient)
//        {
//            PhotonNetwork.LoadLevel("GameScene");
//        }
//        else
//        {
//            // 방장이 아닐 때 이동이 안된다면 텍스트가 힘드니 로그를 찍어보자
//            Debug.Log("방장이 씬을 이동시킬 때 까지 기다리거나, 테스트를 위해 방장이 버튼을 누른다");
//        }
    
