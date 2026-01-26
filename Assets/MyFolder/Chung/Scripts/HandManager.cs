using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    // [설정] 인스펙터에서 인터렉터를 미리 넣어둡니다.
    [SerializeField] private RayInteractor _rayInteractor;
    [SerializeField] private HandGrabInteractor _handInteractor;


    // 이 함수를 인터렉터의 [Select Entered] 또는 [Activate] 이벤트에 연결합니다.
    public void OnInteractAction()
    {
        Debug.Log("[HandManager] On Interact Action is Start");
        // 1. [Select 될 때만 가져옴] 손에 잡고 있는 아이템 확인
        IItem currentItem = null;
        if (_handInteractor.HasInteractable) // Meta SDK 기능 활용
        {
            // 잡은 물체의 인터페이스 추출
            var grabbedObj = _handInteractor.Interactable.transform.gameObject;
            currentItem = grabbedObj.GetComponent<IItem>();
            Debug.Log("Current Grab Item Is :" + currentItem);
        }

        // 2. [Select 될 때만 가져옴] 레이가 보고 있는 리시버 확인
        IReceiver targetReceiver = null;
        if (_rayInteractor.HasInteractable)
        {
            targetReceiver = _rayInteractor.Interactable.gameObject.GetComponent<IReceiver>();
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

    public void DebugHover()
    {
        Debug.Log("[HandManager] OnHover");
    }
}