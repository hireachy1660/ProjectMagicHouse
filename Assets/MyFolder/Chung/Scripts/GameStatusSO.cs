using UnityEngine;
using System;

public enum GameState { Login, Lobby, RoleSelect, InGame, GameOver }
public enum Role { Pathfinder, Inquisitor}

[CreateAssetMenu(fileName = "GameStatus", menuName = "Detective/SO/GameStatus")]
public class GameStatusSO : ScriptableObject
{
    public GameState currentState;

    [Header("Player Role Data")]
    public string myRole; // "Player_A" 또는 "Player_B"

    [Header("Events")]
    public Action<GameState> OnStateChanged;
    public Action OnAllPlayersReady; // 모든 인원이 MyRole을 설정했을 때

    [Header("Scenes Configuration")]
    public string lobbyScene = "LobbyScene";
    public string gameScene = "GameScene";
    public string endingScene = "EndingScene";

    public void ChangeState(GameState newState)
    {
        currentState = newState;
        OnStateChanged?.Invoke(newState);
    }
}
