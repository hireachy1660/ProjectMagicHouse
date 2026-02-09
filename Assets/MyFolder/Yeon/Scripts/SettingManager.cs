using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingManager : MonoBehaviour
{
    // 어디서든 접근 가능하게 싱글톤 처리
    public static SettingManager Instance;

    public GameObject settingCanvas;    // 환경설정 UI
    public Slider bgmSlider;
    public Slider sfxSlider;
    public Button closeBtn;
    public Button ExitBtn;

    public void Awake()
    {
        // 씬이 바뀌어도 이 오브젝트를 파괴하지말라~
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // 씬이 바뀌어도 유지
            settingCanvas.SetActive(false); // 시작할 땐 꺼둠
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // 저장된 소리 값 불러오기 (없으면 1.0)
        bgmSlider.value = PlayerPrefs.GetFloat("BGM_Volume", 1.0f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFX_Volume", 1.0f);

        // 이벤트 연결
        bgmSlider.onValueChanged.AddListener(SetBGM);
        sfxSlider.onValueChanged.AddListener(SetSFX);
        closeBtn.onClick.AddListener(ToggleSettings);   // 닫기 버튼 누르면 토글
        ExitBtn.onClick.AddListener(QuitGame);

    }
    
    // 외부 로그인 버튼, 조이스틱 버튼에서 호출할 함수
    public void ToggleSettings()
    {
        bool isActive = settingCanvas.activeSelf;
        settingCanvas.SetActive(!isActive);
    }

    private void SetBGM(float value)
    {
        PlayerPrefs.SetFloat("BGM_Volume", value);
    }
    private void SetSFX(float value)
    {
        PlayerPrefs.SetFloat("SFX_Volume", value);
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}