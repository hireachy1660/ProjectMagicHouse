using System;
using UnityEngine;
using TMPro;

// 메타 가상 키보드의 추상 핸들러를 상속받습니다.
public class OVRVirtualKeyboardTMPHandler : OVRVirtualKeyboard.AbstractTextHandler
{
    [SerializeField]
    private TMP_InputField inputField;

    // 키보드 시스템에 현재 텍스트 정보를 전달하는 통로
    public override Action<string> OnTextChanged { get; set; }
    public override string Text => inputField ? inputField.text : string.Empty;
    public override bool SubmitOnEnter => inputField && inputField.lineType != TMP_InputField.LineType.MultiLineNewline;
    public override bool IsFocused => inputField && inputField.isFocused;

    // 엔터키 클릭 시 실행
    public override void Submit()
    {
        if (!inputField) return;
        inputField.onEndEdit.Invoke(inputField.text);
        // PUN2 로그인 버튼 클릭과 동일한 로직을 여기에 추가해도 좋습니다.
    }

    // 자판 입력 시 실행
    public override void AppendText(string s)
    {
        if (!inputField) return;
        inputField.text += s;
        // 커서(Caret) 위치를 마지막으로 이동
        MoveTextEnd();
    }

    // 백스페이스 입력 시 실행
    public override void ApplyBackspace()
    {
        if (!inputField || string.IsNullOrEmpty(inputField.text)) return;
        inputField.text = inputField.text.Substring(0, inputField.text.Length - 1);
    }

    // 커서를 텍스트 끝으로 이동
    public override void MoveTextEnd()
    {
        if (!inputField) return;
        inputField.caretPosition = inputField.text.Length;
    }

    protected void Start()
    {
        if (inputField)
        {
            // TMP 값이 바뀔 때마다 키보드에 알림 (자동 완성 등을 위해 필요)
            inputField.onValueChanged.AddListener(s => OnTextChanged?.Invoke(s));
        }
    }
}