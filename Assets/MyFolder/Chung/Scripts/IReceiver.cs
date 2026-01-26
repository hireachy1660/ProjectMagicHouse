/// <summary>
/// 매니저의 명령을 받아 실제 행동(문 열기, 불 켜기)을 수행하는 대상
/// </summary>
public interface IReceiver
{
    // [상황 1] 아이템을 사용했을 때 (예: 열쇠로 문 열기, 프로젝터에 사진 넣기)
    // 매개변수 item: 손에 들고 있던 그 아이템 정보
    void OnReceiveItem(IItem item);

    // [상황 2] 맨손으로 작동시켰을 때 (예: 그냥 버튼 누르기, 레버 당기기)
    void OnActivate();
}