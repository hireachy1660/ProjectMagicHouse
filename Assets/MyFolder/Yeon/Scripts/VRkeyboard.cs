using UnityEngine;
using TMPro;

public class VRKeyboard : MonoBehaviour
{
    private TMP_InputField inputField;
    private TouchScreenKeyboard overlayKeyboard;

    private void Start()
    {
        // 이 스크립트가 붙은 오브젝트의 InputField를 가져온다.
        inputField = GetComponent<TMP_InputField>();

        // 인풋필드를 클릭(선택)했을 때 키보드가 뜨도록 이벤트를 연결한다.
        inputField.onSelect.AddListener(x => OpenKeyboard());
    }

    public void OpenKeyboard()
    {
        // 퀘스트 시스템 키보드를 연다.
        overlayKeyboard = TouchScreenKeyboard.Open(inputField.text, TouchScreenKeyboardType.Default);
    }

    private void Update()
    {
        // overlayKeyboard가 null인지 체크
        if (overlayKeyboard == null) return;

        // 키보드가 활성화된 상태일 때만 텍스트 동기화
        if(overlayKeyboard.active)
        {
            inputField.text = overlayKeyboard.text;
        }
    }
}