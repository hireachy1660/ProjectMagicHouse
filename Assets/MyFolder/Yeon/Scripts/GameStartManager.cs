using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using ExitGames.Client.Photon;
using Photon.Realtime;  // Hashtable 사용

public class GameStartManager : MonoBehaviourPunCallbacks
{
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

    private bool CheckAllPlayersReady()
    {
        // 방에 2명이 안 들어왔으면 시작 불가
        //if (PhotonNetwork.CurrentRoom.PlayerCount < 2) return false;

        // 방에 있는 모든 플레이어를 검사
        foreach(var player in PhotonNetwork.PlayerList)
        {
            // 한 명이라도 가방에 "MyRole"이 없으면 false
            if(!player.CustomProperties.ContainsKey("MyRole"))
            {
                return false;
            }
        }

        // 모두가 "MyRole"를 가지고 있다면 true!
        return true;
    }

    public void ClickStartButton()
    {
        // 모든 준비가 끝났을 때만 씬 이동
        Debug.Log("모두 준비 완료! 게임씬으로 이동한다.");
        PhotonNetwork.LoadLevel("GameScene");
    }
}
