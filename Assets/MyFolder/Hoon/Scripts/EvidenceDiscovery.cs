using UnityEngine;

public class EvidenceDiscovery : MonoBehaviour
{
    public string locationName = "사건 현장";
    [TextArea]
    public string discoveryDescription = "중요한 단서를 발견했습니다.";

    private bool isCaptured = false;

    // 빌딩블록의 Grabbable 이벤트에서 이 함수를 호출하게 설정하세요
    public void CaptureDiscovery()
    {
        if (isCaptured) return; // 딱 한 번만 찍히도록

        ScreenshotHelper helper = FindObjectOfType<ScreenshotHelper>();
        if (helper != null)
        {
            StartCoroutine(helper.CaptureRoutineCustom(locationName, discoveryDescription));
            isCaptured = true;
        }
    }
}