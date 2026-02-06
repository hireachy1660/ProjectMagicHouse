using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;  // Hashtable 사용
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameStartManager : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private GameStatusSO gameStatus;

    public Button startButton;  // 시작 버튼

    private void Start()
    {
        // 시작할 때 한 번 체크
        RefreshStartButton();
    }

    // 누군가 역할을 선택해서 가방이 업데이트될 때마다 자동으로 실행됨
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if(changedProps.ContainsKey("MyRole"))
        {
            RefreshStartButton();
        }
    }

    // 새로운 플레이어가 들어오거나 나갈 때도 체크
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    { RefreshStartButton(); }
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        RefreshStartButton();
    }
    
    private void RefreshStartButton()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            startButton.gameObject.SetActive(true);
            startButton.interactable = CheckAllPlayersReady();
        }
        else
        {
            startButton.gameObject.SetActive(false);
        }
    }
    ////////////////////////////수정된 코드/////////////////////////////////////////
    private bool CheckAllPlayersReady()
    {
        // 1. 인원 체크
        if (PhotonNetwork.CurrentRoom.PlayerCount < 2) return false;

        // 2. 역할 선점 및 중복 체크
        HashSet<string> selectedRoles = new HashSet<string>();

        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties.TryGetValue("MyRole", out object role))
            {
                string roleStr = (string)role;
                // 이미 누군가 선택한 역할이라면 중복!
                if (selectedRoles.Contains(roleStr)) return false;

                selectedRoles.Add(roleStr);
            }
            else
            {
                return false; // 아직 안 고른 사람 있음
            }
        }

        return true; // 2명이 서로 다른 역할을 골랐음
    }

    public void ClickStartButton()
    {
        // 모든 준비가 끝났을 때만 씬 이동
        Debug.Log("모두 준비 완료! 게임씬으로 이동한다.");
        PhotonNetwork.LoadLevel(gameStatus.gameScene);
    }
}
