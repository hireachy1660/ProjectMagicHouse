using UnityEngine;
using System.IO;

public class ScreenshotTaker : MonoBehaviour
{
    public Camera captureCamera; // 목적지에 배치할 카메라
    public int resWidth = 1024;
    public int resHeight = 768;

    [ContextMenu("Take Screenshot")]
    public void TakeScreenshot()
    {
        if (captureCamera == null) captureCamera = Camera.main;

        RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
        captureCamera.targetTexture = rt;
        Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
        captureCamera.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
        captureCamera.targetTexture = null;
        RenderTexture.active = null;
        DestroyImmediate(rt);

        byte[] bytes = screenShot.EncodeToPNG();
        string filename = Path.Combine(Application.dataPath, $"Photo_{System.DateTime.Now:yyyyMMdd_HHmmss}.png");
        File.WriteAllBytes(filename, bytes);

        Debug.Log($"<color=lime>사진 저장 완료: {filename}</color>");
    }
}