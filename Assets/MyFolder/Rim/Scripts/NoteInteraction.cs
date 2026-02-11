using UnityEngine;
using Oculus.Interaction;
using UnityEngine.Events;

public class NoteInteraction : MonoBehaviour
{
    [Header("켜질 UI Panel")]
    public GameObject targetPanel;

    private void Start()
    {
        // 처음엔 Panel 꺼놓기
        if (targetPanel != null)
            targetPanel.SetActive(false);

        // Interactable 이벤트 등록
        var interactable = GetComponentInChildren<InteractableUnityEventWrapper>();
        if (interactable != null)
        {
            interactable.WhenSelect.AddListener(ShowPanel);
        }
    }

    public void ShowPanel()
    {
        if (targetPanel != null)
        {
            targetPanel.SetActive(true);
            Debug.Log("<color=cyan>Note Panel 열림!</color>");
        }
    }

    public void ClosePanel()
    {
        if (targetPanel != null)
        {
            targetPanel.SetActive(false);
            Debug.Log("<color=cyan>Note Panel 닫힘!</color>");
        }
    }
}