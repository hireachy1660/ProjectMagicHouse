using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class EndingCinemaManager : MonoBehaviour
{
    [Header("Data Source")]
    public EndingDataSO endingData; // 새로 만든 엔딩 데이터 SO

    [Header("UI Reference")]
    public CanvasGroup panelGroup;   // 사진과 텍스트를 담은 부모 패널
    public Image photoDisplay;       // 사진이 표시될 UI Image
    public TextMeshProUGUI textDisplay; // 텍스트가 타이핑될 UI Text

    [Header("Settings")]
    public float typingSpeed = 0.05f;  // 타이핑 속도
    public float displayDelay = 3.0f;  // 문장 완성 후 대기 시간
    public float fadeSpeed = 1.5f;     // 페이드 속도

    private void Start()
    {
        // 시작 시 초기화
        panelGroup.alpha = 0;
        textDisplay.text = "";

        // 데이터가 아예 없을 경우를 대비한 PC 테스트용 더미 데이터 (필요 없으면 삭제)
        if (endingData.snapshots.Count == 0)
        {
            Debug.LogWarning("데이터가 비어있어 테스트용 데이터를 생성합니다.");
            endingData.AddSnapshot("사무실", "사건의 재구성을 시작합니다.", null);
            endingData.isEvidenceFound = true;
        }

        StartCoroutine(PlayEndingSequence());
    }

    IEnumerator PlayEndingSequence()
    {
        yield return new WaitForSeconds(1.5f); // 성당 분위기 잡는 시간

        // 1. 범인의 행방 (인트로)
        string introText = endingData.isEvidenceFound
            ? "범인은 시공간의 틈새로 달아났으나, 당신의 추리가 결정적 증거를 남겼습니다."
            : "범인은 종적을 감췄고, 진실은 다시 시간의 저편으로 사라지려 합니다.";

        yield return StartCoroutine(TypeAndFade(introText, null));

        // 2. 플레이어의 행적 (기록된 스냅샷 루프)
        foreach (var shot in endingData.snapshots)
        {
            string content = $"[{shot.locationName}] {shot.achievement}";
            yield return StartCoroutine(TypeAndFade(content, shot.photo));
        }

        // 3. 마지막 마무리 문구 (로아식 낭만)
        string outroText = "사건 번호 #2026-02. 우리의 추격은 여기서 멈추지 않습니다.";
        yield return StartCoroutine(TypeAndFade(outroText, null));

        // 4. 모든 연출 종료 후 처리 (예: 타이틀로 돌아가기)
        Debug.Log("엔딩 시네마 종료");
    }

    // 사진을 갈아끼우고 텍스트를 타이핑하는 핵심 루틴
    IEnumerator TypeAndFade(string message, Sprite photo)
    {
        // 사진 설정 (없으면 기본 배경 유지)
        if (photo != null) photoDisplay.sprite = photo;

        // 페이드 인
        yield return StartCoroutine(FadeCanvas(1f));

        // 타이핑 효과
        textDisplay.text = "";
        foreach (char letter in message.ToCharArray())
        {
            textDisplay.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        // 대기
        yield return new WaitForSeconds(displayDelay);

        // 페이드 아웃
        yield return StartCoroutine(FadeCanvas(0f));
        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator FadeCanvas(float targetAlpha)
    {
        float startAlpha = panelGroup.alpha;
        float time = 0;
        while (time < 1f)
        {
            time += Time.deltaTime * fadeSpeed;
            panelGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time);
            yield return null;
        }
        panelGroup.alpha = targetAlpha;
    }
}