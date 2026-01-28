using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class DebugAutoJoiner : MonoBehaviourPunCallbacks
{
    [Header("테스트 설정")]
    public string testRole = "Player_A"; // 테스트하고 싶은 프리팹 이름
    public bool isDebugMode = true;

    void Start()
    {
        if (!isDebugMode) return;

        // 1. 이미 연결되어 있다면 바로 역할 설정
        if (PhotonNetwork.IsConnectedAndReady)
        {
            SetDebugRole();
        }
        else
        {
            // 2. 연결 안 되어 있으면 서버 접속 시작
            PhotonNetwork.ConnectUsingSettings();
            Debug.Log("[Debug] 포톤 서버 접속 시도...");
        }
    }

    public override void OnConnectedToMaster()
    {
        // 3. 마스터 서버 접속 시 바로 랜덤 방 입장/생성
        PhotonNetwork.JoinOrCreateRoom("DebugRoom", new RoomOptions { MaxPlayers = 2 }, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("[Debug] 테스트 방 입장 완료. 역할을 설정합니다.");
        SetDebugRole();
    }

    private void SetDebugRole()
    {
        // 4. 스포너가 기다리고 있는 가방에 데이터를 강제로 넣음
        Hashtable props = new Hashtable { { "MyRole", testRole } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        Debug.Log($"[Debug] 가방에 {testRole} 주입 완료. 이제 스포너가 작동합니다.");
    }
}