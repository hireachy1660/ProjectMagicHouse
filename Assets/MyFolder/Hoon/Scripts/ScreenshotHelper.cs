using UnityEngine;
using System.Collections;

public class ScreenshotHelper : MonoBehaviour
{
    public GameProgressSO gameProgress;
    public EndingDataSO endingData;

    private string[] progressTexts = {
        "서재의 유산을 깨우고 금고를 열어 새로운 좌표를 확보했습니다.",
        "공터에서 범인의 유일한 흔적인 발자국을 기록했습니다.",
        "수많은 흔적 속에서 일치하는 증거(신발)를 찾아냈습니다.",
        "피 묻은 신발을 화이트보드에 새겨 진실을 완성했습니다!"
    };

    private void Start()
    {
        // 화이트보드 등록(진행도 상승) 시 자동 캡처
        gameProgress.OnEvidenceAdded += (progressIndex) => {
            StartCoroutine(CaptureRoutine(progressIndex));
        };
    }

    // A. 진행도 기반 캡처 (화이트보드용)
    IEnumerator CaptureRoutine(int index)
    {
        yield return new WaitForEndOfFrame();
        CaptureAndSave("사무실", progressTexts[Mathf.Clamp(index, 0, progressTexts.Length - 1)]);
    }

    // B. 아이템 발견 시 직접 호출할 캡처 함수 (아이템 잡기용)
    public IEnumerator CaptureRoutineCustom(string location, string desc)
    {
        yield return new WaitForEndOfFrame();
        CaptureAndSave(location, desc);
    }

    // 공통 저장 로직
    private void CaptureAndSave(string location, string desc)
    {
        Texture2D screenShot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        screenShot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenShot.Apply();

        Sprite photoSprite = Sprite.Create(screenShot, new Rect(0, 0, screenShot.width, screenShot.height), new Vector2(0.5f, 0.5f));
        endingData.AddSnapshot(location, desc, photoSprite);

        Debug.Log($"<color=cyan>[저장완료]</color> {location}: {desc}");
    }
}