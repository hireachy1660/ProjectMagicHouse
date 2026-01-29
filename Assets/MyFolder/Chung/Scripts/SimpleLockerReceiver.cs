using System.Collections;
using System.Net.Mail;
using UnityEngine;
using Photon.Pun;

public class SimpleLockerReceiver : MonoBehaviourPun, IReceiver
{
    [Header("Settings")]
    [SerializeField] private string _requiredKeyID; // 예: "Key_Locker"
    [SerializeField] private Transform _doorObject; // 움직일 오브젝트
    [SerializeField] private Transform attachPoint;
    [SerializeField] private Vector3 _moveOffset = new Vector3(0.5f, 0, 0); // 열릴 때 이동할 거리/방향
    [SerializeField] private float _moveSpeed = 2.0f; // 열리는 속도


    private Vector3 _closedPosition;
    private Vector3 _openPosition;
    private bool isOpen = false;

    private void Awake()
    {
        // 시작 시점의 위치를 '닫힌 위치'로 저장
        if (_doorObject != null)
        {
            _closedPosition = _doorObject.localPosition;
            _openPosition = _closedPosition + _moveOffset;
        }
    }

    // 인터페이스 구현: 아이템을 받았을 때
    public void OnReceiveItem(IItem _item)
    {
        if (isOpen) return; // 이미 열렸으면 패스

        if (_item.ItemID == _requiredKeyID)
        {
            Debug.Log($" 사물함 잠금 해제! ({_item.ItemID})");

            //StartCoroutine(StartOpen(_item.Transform.parent.transform));
            photonView.RPC(nameof(StartOpenCouroutine), RpcTarget.AllBuffered, _item.PhotonViewID);
        }
        else
        {
            Debug.Log($" 맞는 열쇠가 아닙니다. (필요: {_requiredKeyID})");
        }
    }

    [PunRPC]
    private void StartOpenCouroutine(int _ViewID)
    {
        if( isOpen) return;

        isOpen = true; // 열림 상태로 전환 -> 코루틴에서 문 이동함

        StartCoroutine(StartOpen(_ViewID));


    }

    private IEnumerator StartOpen(int _ViewID)
    {
        // 상태에 따라 목표 위치 결정
        if (_doorObject == null) yield break;
        // Vector3 targetPos = _isOpen ? _openPosition : _closedPosition;

        // 포톤 뷰 아이디에서 다시 아이템으로 변환
        PhotonView targetView = PhotonView.Find(_ViewID);
        if (targetView == null) yield break;
        IItem item = targetView.GetComponent<IItem>();
        if (item == null) yield break;

        // 아이템의 상호작용 중지 메소드 호출
        // 현재 언 그랩에서 키네매틱을 켜는데 물건이 놓아 지면서 한번 더 호출이 되서 문제 가 됨
        item.OnPlaced();

        // 아이템을 어테치 포인트의 자식으로 지정 후 위치 조정
        item.Transform.SetParent(attachPoint);
        item.Transform.localPosition = Vector3.zero;
        item.Transform.rotation = Quaternion.identity;

        float elapcedTime = 0f;

        // 에니메이션 재생
        while ( elapcedTime <= _moveSpeed)
        {
            _doorObject.localPosition = Vector3.Lerp(_doorObject.localPosition, _openPosition, Time.deltaTime * _moveSpeed);
            elapcedTime += Time.deltaTime;
            yield return null;
        }

    }

    // 인터페이스 구현: 맨손으로 눌렀을 때
    [PunRPC]
    public void OnActivate()
    {
        if (!isOpen)
        {
            Debug.Log("잠겨있습니다. 덜컹덜컹.");
            // 여기에 '덜컹'거리는 효과음이나 살짝 움직이는 연출 추가 가능
        }
    }
}