using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using UnityEngine;
using System.Collections;

public class HandManager : MonoBehaviour
{
    // [설정] 인스펙터에서 인터렉터를 미리 넣어둡니다.
    [SerializeField] private RayInteractor rayInteractor;
    [SerializeField] private GrabInteractor handInteractor;


    // 이 함수를 인터렉터의 [Select Entered] 또는 [Activate] 이벤트에 연결합니다.
    public void OnInteractAction()
    {
        StartCoroutine(WaitAndInteractAction());
    }

    private IEnumerator WaitAndInteractAction()
    {
        Debug.Log("[HandManager] On Interact Action is Start");

        yield return new WaitForEndOfFrame();
        // 1. [Select 될 때만 가져옴] 손에 잡고 있는 아이템 확인
        IItem currentItem = null;
        if (handInteractor.HasSelectedInteractable) // Meta SDK 기능 활용
        {
            // 잡은 물체의 인터페이스 추출
            currentItem = handInteractor.SelectedInteractable.gameObject.transform.parent.GetComponent<IItem>();
            Debug.Log("Current Grab Item Is :" + currentItem + currentItem.Transform.gameObject.name);
        }
        else
        {
            Debug.Log("[Hand Manager] Grab Interactor Can't Found Candidate and Selected Interactable ");
        }

        // 2. [Select 될 때만 가져옴] 레이가 보고 있는 리시버 확인
        IReceiver targetReceiver = null;
        if (rayInteractor.HasSelectedInteractable)
        {
            targetReceiver = rayInteractor.Interactable.gameObject.transform.parent.GetComponent<IReceiver>();
            Debug.Log("[Hand Manager] Ray Interactor Found Candidate and Selected Interactable " + rayInteractor.SelectedInteractable);
        }
        else
        { 
            Debug.Log("[Hand Manager] Ray Interactor Can't Found Candidate and Selected Interactable " + rayInteractor.HasSelectedInteractable);
        }

        // 3. [작동] 매니저가 리시버의 메소드를 직접 호출
        if (targetReceiver != null)
        {
            if (currentItem != null)
            {
                // "열쇠를 쥔 채로 문을 쐈다" -> 문아, 이 열쇠 받아라.
                targetReceiver.OnReceiveItem(currentItem);
            }
            else
            {
                // "맨손으로 버튼을 눌렀다" -> 버튼아, 작동해라.
                targetReceiver.OnActivate();
            }
        }

    }

    [Header("Debug Settings")]
    [SerializeField] private bool _showRayInScene = true;

    void Update()
    {
        if (rayInteractor == null) return;

        // 1. 물리적 인식 상태 (Candidate) 확인
        if (rayInteractor.HasCandidate)
        {
            var target = rayInteractor.Candidate;
            Debug.Log($"<color=cyan>[Ray-Candidate]</color> 감지됨: {target.gameObject.name} (거리: {Vector3.Distance(rayInteractor.Origin, rayInteractor.End):F2}m)");
        }

        // 2. 논리적 선택 상태 (Selected) 확인
        if (rayInteractor.HasSelectedInteractable)
        {
            Debug.Log($"<color=lime>[Ray-Selected]</color> 선택 완료: {rayInteractor.SelectedInteractable.gameObject.name}");
        }

        // 3. 트리거 입력 상태 (Selector) 확인
        // 실제 버튼 입력에 의해 '작동' 신호를 보냈는지 확인합니다.

            // ISDK의 Selector 상태는 내부적으로 관리되므로, 인터렉터의 State 변화로 추론합니다.
            if (rayInteractor.State == InteractorState.Select)
            {
                Debug.Log("<color=yellow>[Ray-Selector]</color> 현재 트리거(Selector)에 의해 'Select' 상태 유지 중");
            }
        

        // 4. 씬 뷰에서 레이 시각화 (인게임 선이 안 보일 때 유용)
        if (_showRayInScene)
        {
            Debug.DrawLine(rayInteractor.Origin, rayInteractor.End, Color.red);
        }
    }
}