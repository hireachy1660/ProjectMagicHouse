using UnityEngine;
using Photon.Pun;

public class GameSceneManager : MonoBehaviourPunCallbacks
{
    [Header("Data Hubs")]
    [SerializeField] private GameStatusSO gameStatus;
    [SerializeField] private GameProgressSO gameProgress;

    private void Start()
    {
        // 1. 게임 씬 진입 즉시 상태를 'InGame'으로 확정하여 정보의 신뢰성 확보
        if (gameStatus != null)
        {
            gameStatus.ChangeState(GameState.InGame);
            Debug.Log("<color=blue>[GameManager]</color> 현재 게임 상태: InGame");
        }

        // 2. 화이트보드 진행도(GameProgressSO)의 클리어 이벤트를 구독
        if (gameProgress != null)
        {
            gameProgress.OnStageClear += HandleStageClear;
            gameProgress.ResetProgress(); // 씬 시작 시 진행도 초기화 
        }
    }

    private void OnDestroy()
    {
        // 메모리 누수 방지를 위한 이벤트 구독 해제
        if (gameProgress != null)
        {
            gameProgress.OnStageClear -= HandleStageClear;
        }
    }

    private void HandleStageClear()
    {
        Debug.Log("<color=green>[GameManager]</color> 모든 증거 수집 완료! 엔딩으로 전환합니다.");

        // 3. 상태를 GameOver(혹은 Clear)로 변경 
        gameStatus.ChangeState(GameState.GameOver);
        // 4. 방장(MasterClient)이 대표로 엔딩 씬 이동을 실행
        if (PhotonNetwork.IsMasterClient)
        {
            // GameStatusSO에 설정된 엔딩 씬 문자열 사용
            PhotonNetwork.LoadLevel(gameStatus.endingScene);
        }
    }
}
