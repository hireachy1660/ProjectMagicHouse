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
        closeBtn.onClick.AddListener(CloseSetting);   // 닫기 버튼 누르면 토글
        ExitBtn.onClick.AddListener(QuitGame);

    }

    private void Update()
    {
        // 왼쪽 컨트롤러의 Y버튼 감지
        if(OVRInput.GetDown(OVRInput.RawButton.Y))
            {
            if(settingCanvas.activeSelf)
            {
                CloseSetting();
            }
            else
            {
                OpenSetting();
                // VR에서는 창이 눈앞에 나타나는 것이 중요하다.
                PositionCanvasInFront();
            }

        }
    }
    
    // 캔버스를 카메라 앞으로 소호나하는 함수
    private void PositionCanvasInFront()
    {
        // OVRCameraRig 내의 CenterEyeAnchor를 찾는다.
        Transform camTransform = Camera.main.transform;

        // 카메라 앞 1.2m 지점에 배치
        Vector3 targetPos = camTransform.position + (camTransform.forward * 1.2f);
        settingCanvas.transform.position = targetPos;

        // UI가 사용자를 정면으로 바라보게 회전
        settingCanvas.transform.LookAt(camTransform.position);
        settingCanvas.transform.Rotate(0, 180, 0);
    }

    // 외부 로그인 버튼, 조이스틱 버튼에서 호출할 함수
    public void OpenSetting()
    {
        Debug.Log("버튼 눌림!");        // 이 글자가 콘솔창에 뜨는지 확인

        // 꺼져있든 켜져있든 무조건 킨다.
        if (settingCanvas != null)
        {
            settingCanvas.SetActive(true);
            Debug.Log("환경설정 창 활성화");
        }
    }

    public void CloseSetting()
    {
        // 무조건 끈다.
        if(settingCanvas != null)
        {
            settingCanvas.SetActive(false);
            Debug.Log("환경설정 창 비활성화");
        }
    }

    private void SetBGM(float value)
    {
        PlayerPrefs.SetFloat("BGM_Volume", value);
    }
    private void SetSFX(float value)
    {
        PlayerPrefs.SetFloat("SFX_Volume", value);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}