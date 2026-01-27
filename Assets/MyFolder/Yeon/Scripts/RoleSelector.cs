using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;

public class RoleSelector : MonoBehaviourPunCallbacks
{
    // 버튼에 이 함수 연결 ( 인자 값으로 프리팹 이름을 직접 쓴다 )
    public void SelectRoleAndStart(string characterName)
    {
        // 이름이 비었는지 체크(방어코드)
        if (string.IsNullOrEmpty(characterName))
        {
            Debug.LogError("버튼 인스펙터 창에 캐릭터 이름을 안 적으셨어요!");
            return;
        }

        // 가방에 확실히 저장
        Hashtable props = new Hashtable { { "MyRole", characterName } };
        // 내 가방에 데이터를 넣는다.
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        Debug.Log(characterName + "정보를 가방에 넣었습니다!");

        // 포톤 서버가 '가방 업데이트 완료했어'라고 알려주는 함수이다.
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
    
}