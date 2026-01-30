using UnityEngine;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;

public class KeyboardTrigger : MonoBehaviour
{
    [SerializeField]
    private GameObject keyBoard = null;

    public void ShowKeyboard()
    {
        // VR 시스템 키보드 호출 (가장 안정적인 방식)
        Debug.Log("[KeyboardTrigger] ShowKeyBoard on");
        keyBoard.SetActive(false);
    }
}