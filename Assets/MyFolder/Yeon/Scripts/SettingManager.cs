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
    }
    // [상황 1] 로그인 패널 버튼을 눌렀을 때 (화면 중앙 소환)
    public void OpenSettingUI()
    {
        settingCanvas.SetActive(true);
        // 로그인 씬에서는 보통 카메라 정면 적당한 곳에 배치
        settingCanvas.transform.position = new Vector3(0, 1, 2); // 예시 위치
        settingCanvas.transform.LookAt(Camera.main.transform);
        settingCanvas.transform.Rotate(0, 180, 0);
    }

    // [상황 2] VR 컨트롤러 버튼을 눌렀을 때 (손앞에 소환)
    public void OpenSettingVR(Transform controllerTransform)
    {
        settingCanvas.SetActive(true);
        // 컨트롤러 위치 기준 앞쪽 0.5m
        Vector3 spawnPos = controllerTransform.position + (controllerTransform.forward * 0.5f);
        settingCanvas.transform.position = spawnPos;

        // 유저를 바라보게 회전
        settingCanvas.transform.LookAt(Camera.main.transform);
        settingCanvas.transform.Rotate(0, 180, 0);
    }


    public void CloseSetting() => settingCanvas.SetActive(false);

    public void OnBGMChanged(float value)
    {
        //ApplyVolume(value);
        PlayerPrefs.SetFloat("BGM_Volume", value);
    }
    public void OnSFXChanged(float value)
    {
        // 효과음 볼륨 저장
        PlayerPrefs.SetFloat("SFX_Volume", value);

        // 실제 효과음 오디오 소스들에 적용 (AudioMixer 사용 시)
        // mixer.SetFloat("SFXVolume", Mathf.Log10(value) * 20 );
    }

    private void ApplyVolume(float volume)
    {
        // 실제 오디오 믹서나 오디오 소스에 불륨 적용하는 로직
        AudioListener.volume = volume;
    }



}