using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using ExitGames.Client.Photon;
using System.Collections.Generic;
using TMPro;

// 역할패널의 UI담당
public class RoleSelectionManager : MonoBehaviourPunCallbacks
{
    [Header("Buttons")]
    public Button PathfinderBtn;   // 패스파인더 역할 A 버튼
    public Button InquisitorBtn;   // 인쿼지터 역할 B 버튼
    public Button cancelConfirmedBtn;    // 확정 취소 버튼
    public Button startButton;  // 방장 전용 시작 버튼
    public Button readyButton;  // 게스트 전용 준비 버튼
    public Button leaveRoomButton;  // 방 나가기 버튼

    [Header("Confirm UI")]
    public GameObject confirmPanel; // 역할 선택 확정 UI 팝업창
    public TMP_Text confirmText;
    private string selectedRoleName;    // 팝업창에서 잠시 보관용

    [Header("UI Panels")]
    public GameObject lobbyPanel;
    public GameObject roleSelectPanel;

    [Header("So")]
    [SerializeField]
    private GameStatusSO gameStatus;
        
    private void Start()
    {
        // 인스펙터 연결 확인(방어 코드)
        if (confirmPanel == null) return;

        confirmPanel.SetActive(false);

        // 방에 있을 때만 UI 갱신 시도
        if (PhotonNetwork.InRoom)
        {
            RefreshUI();
        }
    }

    // 역할 버튼 클릭 (팝업 띄우기)
    public void OnClickRoleButton(string roleName)
    {
        selectedRoleName = roleName;
        confirmText.text = roleName + "를 선택하시겠습니까?";
        confirmPanel.SetActive(true);
    }

    // 팝업창에서 '예' 확정
    public void ConfirmSelection()
    {
        Hashtable props = new Hashtable
        {
            {"MyRole", selectedRoleName }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        gameStatus.myRole = selectedRoleName;
        confirmPanel.SetActive(false);
    }

    // 팝업창에서 '아니오' 단순 닫기
    public void CancelSelection()
    {
        selectedRoleName = "";
        confirmPanel.SetActive(false);
    }

    // 확정 취소 버튼( 무르기 )
    public void UndoConfirmedRole()
    {
        // 가방에서 역할과 준비 상태 모두 제거 ( null을 넣으면 키가 삭제됨 )
        Hashtable props = new Hashtable
        {
            {"MyRole", null }, {"IsReady", null }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        Debug.Log("역할 선택을 취소하였습니다.");

    }

    // 서버 가방 업데이트 시 호출
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
       if(changedProps.ContainsKey("MyRole"))
        {
            RefreshUI();

            // 내가 역활 확정 팝업을 띄워놨는데 상대가 그 역할을 확정했다면?
            if(confirmPanel.activeSelf && selectedRoleName == (string)changedProps["MyRole"])
            {
                // 상대방이 먼저 선택 했다면
                if(!targetPlayer.IsLocal)
                {
                    Debug.Log("상대방이 먼저 선점했습니다!");
                    CancelSelection();      // 내 팝업 닫기
                }
            }
        }
    }
    // UI 갱신 로직
    private void RefreshUI()
    {
        // 방에 없거나 버튼이 하나라도 연결 안되어 있다면 실행 중단
        if (!PhotonNetwork.InRoom || PathfinderBtn == null || InquisitorBtn == null) return;

        // 누가 어떤 역할을 가져갔는지 확인
        HashSet<string> takenRoles = new HashSet<string>();
        string myRole = (string)PhotonNetwork.LocalPlayer.CustomProperties["MyRole"];
        bool iHaveRole = !string.IsNullOrEmpty(myRole);
        bool iAmReady = PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("IsReady")
            && (bool)PhotonNetwork.LocalPlayer.CustomProperties["IsReady"];
        
        foreach(Player p in PhotonNetwork.PlayerList)
        {
            if (p.CustomProperties.ContainsKey("MyRole") && p.CustomProperties["MyRole"] != null)
            {
                takenRoles.Add((string)p.CustomProperties["MyRole"]);
            }
        }

        // 버튼 활성화 규칙
        // 1. 상대가 고르지 않은 역할이어야 함( !takenRoles.Contains )
        // 2. 나도 아직 아무 역할을 고르지 않은 상태여야 함( !iHaveRole )
        PathfinderBtn.interactable = !takenRoles.Contains("Pathfinder") && !iHaveRole;
        InquisitorBtn.interactable = !takenRoles.Contains("Inquisitor") && !iHaveRole;

        // 확정 취소 버튼 : 내가 역할을 골랐을 때만 활성화
        if(cancelConfirmedBtn != null)
        {
            // 버튼은 항상 보이게 유지
            cancelConfirmedBtn.gameObject.SetActive(true);
            // 내가 역할을 골랐을 때만 클릭 가능
            cancelConfirmedBtn.interactable = iHaveRole;
        }

        // 방장/게스트 버튼 제어
        bool allReady = CheckAllPlayersReady();
        if(PhotonNetwork.IsMasterClient)
        {
            startButton.gameObject.SetActive(true);     // 항상보이게
            readyButton.gameObject.SetActive(false);    // 방장은 레이버튼 없으니 꺼둠
            // 모든 사람이 역할 선택 + 게스트 준비 완료 버튼 시 스타트 버튼 활성화
            startButton.interactable = allReady;
        }
        else
        {
            // 게스트
            startButton.gameObject.SetActive(false);
            readyButton.gameObject.SetActive(true);
            // 역할을 선택했고, 아직 준비 버튼을 안눌렀을 때만 준비 버튼 활성화
            readyButton.interactable = iHaveRole && !iAmReady;
        }
    }

    // 모두 준비되었는지 확인하는 조건
    private bool CheckAllPlayersReady()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount < 2) return false;
        foreach(Player p in PhotonNetwork.PlayerList)
        {
            // 역할이 없는 사람이 있으면 안됨
            if (!p.CustomProperties.ContainsKey("MyRole") || p.CustomProperties["MyRole"] == null )
            {
                return false;
            }

            // 게스트인데 준비 완료를 안 눌렀으면 안됨
            if(!p.IsMasterClient)
            {
                if(!p.CustomProperties.ContainsKey("IsReady") || !(bool)p.CustomProperties["IsReady"])
                {
                    return false;
                }
            }
        }
        return true;
    }

    // 방나나기
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }
    // 게임 시작
    public override void OnLeftRoom()
    {
        Debug.Log("방에서 퇴장하였습니다. 로비UI 정리합니다.");
        // 모든 패널을 끈다. - 확실한 초기화를 하기 위해 한번 더 끈다.
        if(roleSelectPanel != null)
        {
            roleSelectPanel.SetActive(false);
        }
        if(confirmPanel != null)
        {
            confirmPanel.SetActive(false);
        }
        // 로기 패널 켜기
        if( lobbyPanel != null)
        {
            lobbyPanel.SetActive(true);
        }

    }
    public void ClickStartButton()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(gameStatus.gameScene);
        }
    }
    public void ClickReadyButton()
    {
        // 내 가방에 "IsReady" 상태를 true로 저장
        Hashtable props = new Hashtable
        {
            {
                "IsReady", true
            }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        Debug.Log("게스트 준비 완료!");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("방에 입장 완료. 이전 데이터를 지우고 UI를 셋팅합니다");

        // 방장, 게스트 입장하자마자 가방을 비운다.
        Hashtable props = new Hashtable
        {
            {"MyRole", null },{"IsReady", null}
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        // 이제 깨끗한 가방 상태로 UI를 그린다.
        RefreshUI();
    }

    // 방장이 튕겼거나 나갔을 경우 게스트가 방장되는 코드
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log($"방장이 변경되었습니다! 새로운 방장: {newMasterClient.NickName}");
    
        // 방장이 바뀌었으니 UI다시 그리기 - 레디버튼 대신 스타트 버튼으로 
        RefreshUI();
    }
}