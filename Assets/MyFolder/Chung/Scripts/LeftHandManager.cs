using UnityEngine;
using Oculus.Interaction;
using TMPro;
using Oculus.Interaction.HandGrab;
using System.Collections;

public class LeftHandManager : MonoBehaviour
{
    [Header("SDK References")]
    [SerializeField] private HandGrabUseInteractor leftGrabUseInteractor; // 왼손 인터랙터

    [Header("Data & UI")]
    [SerializeField] private EvidenceDatabase database; // 텍스트가 담긴 SO의 데이터 베이스
    [SerializeField] private TextMeshProUGUI titleTMP;
    [SerializeField] private TextMeshProUGUI descTMP;
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private Transform leftHandTr;
    [SerializeField] private Transform CameraRig;

    private void Awake()
    {
        infoPanel.SetActive(false);
    }

    // Hand Grab Use Interactor를 참조하는 Event Wrapper의 'On Select' 이벤트에 이 함수를 연결
    public void OnLeftHandUse()
    {
        if (!leftGrabUseInteractor.Interactable) return;

        // 1. 현재 왼손이 상호작용 중인 인터렉터블 확인
        var interactable = leftGrabUseInteractor.SelectedInteractable;
        if (interactable == null) return;
        Debug.Log($"[LeftHandManager] now Interactor's SelectedIntertorble Is {interactable.gameObject.name}");

        // 2. 해당 물체에서 증거 정보(ID)를 가져옴
        IItem evidence = interactable.gameObject.transform.parent.GetComponent<IItem>();
        if (evidence != null)
        {
            //Vector3 panelPos = leftHandTr.position + transform.position + (transform.forward * 2f);
            //infoPanel.transform.position = new Vector3(leftHandTr.position.x, leftHandTr.position.y, transform.position.z);
            UpdateUI(evidence.ItemID);
        }
    }

    private void UpdateUI(string id)
    {
        // 3. 데이터베이스에서 ID로 검색하여 텍스트 할당 
        var data = database.Get(id);
        if (data != null)
        {
            titleTMP.text = data.title;
            descTMP.text = data.description;
            infoPanel?.SetActive(true);

            StartCoroutine(UILookAtPlayer());
        }
    }

    private IEnumerator UILookAtPlayer()
    {
        while (CameraRig != null && infoPanel.activeSelf)
        {
            // 1. 먼저 카메라를 바라보게 합니다.
            infoPanel.transform.LookAt(CameraRig.transform.position);

            // 2. UI 텍스트가 정면으로 보이도록 Y축으로 180도 추가 회전시킵니다.
            infoPanel.transform.Rotate(0, 180, 0);

            yield return null;
        }
    }

    public void UnLeftHandUse()
    {
        StopCoroutine(UILookAtPlayer());
        infoPanel?.SetActive(false);
    }
}