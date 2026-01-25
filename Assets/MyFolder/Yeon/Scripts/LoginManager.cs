using Photon.Pun;
using Photon.Realtime;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class LoginManager : MonoBehaviourPunCallbacks
{
    [Header("UI Panels")]
    public GameObject loginPanel;
    public GameObject lobbyPanel;

    [Header("Login UI")]
    public TMP_InputField idInputField;
    public TextMeshProUGUI statusText;

    [Header("Lobby UI")]
    public TMP_InputField roomNameInputField;   // 방 이름 입력칸
    public Transform roomListContent;           // scroll view의 방 리스트
    public GameObject roomItemPrefab;           // 방 버튼 프리팹
    private void Start()
    {
        // 게임이 시작되자마자 자동로그인을 시도한다.
        // Login();

        loginPanel.SetActive(true);
        lobbyPanel.SetActive(false);

        Debug.Log("아이디 입력후 버튼이나 키보드 L을 누르세요");
    }
    private void Update()
    {
        // 성능 문제로 버튼이 안눌러졌을떄 위한 강제 로그인 키보드
        if(Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log("L키 감지 : 강제 로그인을 시도합니다.");
            Login();
            
        }
    }
    // 메세지 업데이트하는 공용 함수
    private void UpdateStatus(string message)
    {
        if(statusText != null) statusText.text = message;
        Debug.Log(message);
    }
    public void Login()
    {
        // InputField에서 사용자 ID를 가져온다.
        string userCustomID = idInputField.text;

        // 아이디 있는지 확인
        if(string.IsNullOrEmpty(userCustomID))
        {
            Debug.LogWarning("아이디가 비어있습니다!");
            return;
        }

        // 이미 완전히 연결되어 로비에 있다면 바로 패널 교체
        if(PhotonNetwork.InLobby)
        {
            UpdateStatus("이미 로비에 접속되어 있습니다.");
            loginPanel.SetActive(false);
            lobbyPanel.SetActive(true);
            return;
        }

        // PlayFab 로그인 시작
        var request = new LoginWithCustomIDRequest
        {
            // 사용자 아이디를 PlayFab CustomId로 사용
            CustomId = userCustomID,   // 기기마다 가진 고유 번호
            CreateAccount = true    // 계정이 없으면 새로 만들어라
        };

        Debug.Log($"{userCustomID} 아이디로 PlayFab 로그인을 시도합니다.");

        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
    }

    // ----- PlalyFab 연결 관련 ------

    // 로그인 성공 시 실행
    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("PlayFab 로그인 성공!");

        // 여기서 다음 단계(포톤 서버 접속)으로 넘어가면 된다.
        // 1. 포톤 서버 접속 시작
        ConnectToPhoton();
    }

    // 로그인 실패 시 실행
    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogWarning("로그인 실패: " + error.GenerateErrorReport());
    }

    // ----- 포톤 연결 관련 -----
    private void ConnectToPhoton()
    {
        Debug.Log("포톤 서버 접속 중..");
        // 설정한 AppID를 기반으로 마스터 서버에 접속
        PhotonNetwork.ConnectUsingSettings();

    }

    // 포톤 마스터 서버 접속 성공 시 호출
    // override : 부모 클래스가 가진 기능을 내가 다시 정의하겠다 의미
    public override void OnConnectedToMaster()
    {
        Debug.Log("포톤 마스터 서버 접속 완료!");
        // 2. 로비 입장( 방 목록을 보거나 만들 수 있는 단계 )
        // 로비에 접속해야 방 목록을 확인하거나 방에 들어갈 수 있다.
        PhotonNetwork.JoinLobby();
    }

    // 로비 입장 성공 시 호출
    public override void OnJoinedLobby()
    {
        Debug.Log("포톤 로비 입장 완료! 이제 멀티플레이 준비가 끝났습니다.");
        UpdateStatus("Photon good");
        // 테스트를 위해 바로 방을 만들거나 들어가고 싶다면 아래 주석을 해제.
        //PhotonNetwork.JoinOrCreateRoom("Room1", new RoomOptions { MaxPlayers = 2 }, TypedLobby.Default);

        // "MyRoom"이라는 이름의 방을 접속하거나, 없으면 만든다.
        //PhotonNetwork.JoinOrCreateRoom("MyRoom", new RoomOptions { MaxPlayers = 2}, TypedLobby.Default);

        // UI 패널 교체
        loginPanel.SetActive(false);
        lobbyPanel.SetActive(true);

        // [[[[테스트후 지우기]]] 가짜 방 목록 생성 (개발용 테스트)
        CreateFakeRoomList();

    }

    // ----- 방 관리 -----

    // 방목록이 업데이트될 때 포톤이 호출하는 유일한 함수
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // 기존 UI 아이템들 삭제 - 미리만튼프리팹 볼려고 , 테스끝나고 주석 지우기
        //foreach(Transform child in roomListContent)
        //{
        //    Destroy(child.gameObject);
        //}

        // 새로운 목록 생성
        if(roomList.Count == 0) 
        {
            UpdateStatus("Room Zero");
        }
        else
        {
            foreach (RoomInfo room in roomList)
            {
                // 삭제된 방은 건너뜀
                if (room.RemovedFromList) continue;

                // 방 프리팹 생성
                GameObject item = Instantiate(roomItemPrefab, roomListContent);
                // RoomItem 스크립트가 프리팹에 있어야함
                item.GetComponent<RoomItem>().SetInfo(room, this);
            }
        }
    }


    // 방 만들기 버튼에 연결할 함수
    public void CreateRoom()
    {
        string roomName = roomNameInputField.text;
        if (string.IsNullOrEmpty(roomName))
        {
            roomName = "Room_" + Random.Range(100, 999);
        }

        UpdateStatus($"방 생성 중: {roomName}");
        RoomOptions roomOptions = new RoomOptions { MaxPlayers = 2 };
        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }
    // 방 목록이 변할 때 마다 실행되는 포톤 콜백
    
    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"방 '{PhotonNetwork.CurrentRoom.Name}'입장 완료! 아바타를 생성합니다.");
        UpdateStatus($"방 '{PhotonNetwork.CurrentRoom.Name}'입장 완료! 아바타를 생성합니다.");
        // Resources 폴더에 있는 프리팹 이름을 "MyAvatar"라고 가정했을 때:
        // 생성 위치는 Vector3.zero, 회전은 기본값
        PhotonNetwork.Instantiate("NetworkPrefabs/MyAvatar", Vector3.zero, Quaternion.identity);

        // 필요하다면 여기서 로비 패널을 끄거나 씬 전환
        lobbyPanel.SetActive(false);
    }

    // 접속 실패 시 호출되는 콜백
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogError($"포톤 연결 실패: {cause.ToString()}");
    }

    // [[[ 테스트 후 지우기 ]]] - 가짜방 만들기
    public void CreateFakeRoomList()
    {
        // 기존 UI 아이템들 삭제
        foreach (Transform child in roomListContent)
        {
            Destroy(child.gameObject);
        }

        // 가짜 데이터 3개 생성
        for (int i = 1; i <= 3; i++)
        {
            GameObject item = Instantiate(roomItemPrefab, roomListContent);

            // RoomItem 스크립트를 가져와서 수동으로 텍스트 설정
            // (SetInfo 대신 직접 텍스트 컴포넌트에 접근하는 방식이 테스트에 편합니다)
            RoomItem script = item.GetComponent<RoomItem>();
            if (script != null)
            {
                // RoomItem 스크립트에 텍스트 변수가 public으로 되어 있어야 합니다.
                // 만약 아래 변수명이 다르면 본인의 RoomItem 변수명으로 고치세요.
                script.roomInfoText.text = "Test Room " + i;
            }
        }
    }
}
