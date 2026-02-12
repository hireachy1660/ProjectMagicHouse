using UnityEngine;
using System.Collections;

public class ScreenshotHelper : MonoBehaviour
{
    public GameProgressSO gameProgress; // 기존 사용하던 SO
    public EndingDataSO endingData;     // 새로 만든 엔딩 SO

    // 각 진행도(index)에 매칭될 텍스트들
    private string[] progressTexts = {
        "사건의 시작, 사무실에서 첫 단서를 찾았습니다.",
        "과거의 서재에서 비밀 열쇠를 확보했습니다.",
        "시장의 혼란 속에서 범인의 흔적을 쫓았습니다.",
        "범인의 은신처, 금고를 열어 결정적 증거를 찾았습니다!"
    };

    private void Start()
    {
        // 페이지가 넘어갈 때마다 Capture 함수 실행
        gameProgress.OnEvidenceAdded += (progressIndex) => {
            StartCoroutine(CaptureRoutine(progressIndex));
        };
    }

    IEnumerator CaptureRoutine(int index)
    {
        // UI가 바뀌고 화면이 갱신될 시간을 아주 잠깐 줌 (0.1초)
        yield return new WaitForEndOfFrame();

        // 1. 화면 캡처
        Texture2D screenShot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        screenShot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenShot.Apply();

        // 2. Sprite로 변환 (UI 표시용)
        Sprite photoSprite = Sprite.Create(screenShot, new Rect(0, 0, screenShot.width, screenShot.height), new Vector2(0.5f, 0.5f));

        // 3. EndingDataSO에 저장
        string location = "사건 현장"; // 필요시 index에 따라 분기 처리 가능
        string desc = (index < progressTexts.Length) ? progressTexts[index] : "중요한 단서를 기록했습니다.";

        endingData.AddSnapshot(location, desc, photoSprite);

        // 마지막 인덱스라면 증거 발견 플래그 세움
        if (index >= 3) endingData.isEvidenceFound = true;

        Debug.Log($"<color=cyan>[Capture]</color> {index}번 진행도 사진 저장 완료!");
    }
}