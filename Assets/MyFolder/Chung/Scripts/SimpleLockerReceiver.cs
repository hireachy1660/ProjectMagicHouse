using System.Collections;
using System.Net.Mail;
using UnityEngine;
using Photon.Pun;

public class SimpleLockerReceiver : MonoBehaviour, IReceiver
{
    [Header("Settings")]
    [SerializeField] private string _requiredKeyID; // 예: "Key_Locker"
    [SerializeField] private Transform _doorObject; // 움직일 오브젝트
    [SerializeField] private Transform attachPoint;
    [SerializeField] private Vector3 _moveOffset = new Vector3(0.5f, 0, 0); // 열릴 때 이동할 거리/방향
    [SerializeField] private float _moveSpeed = 2.0f; // 열리는 속도


    private Vector3 _closedPosition;
    private Vector3 _openPosition;
    private bool _isOpen = false;

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
        if (_isOpen) return; // 이미 열렸으면 패스

        if (_item.ItemID == _requiredKeyID)
        {
            Debug.Log($" 사물함 잠금 해제! ({_item.ItemID})");
            _isOpen = true; // 열림 상태로 전환 -> Update에서 문 이동함

            _item.OnPlaced();
            StartCoroutine(StartOpen(_item.Transform.parent.transform));
        }
        else
        {
            Debug.Log($" 맞는 열쇠가 아닙니다. (필요: {_requiredKeyID})");
        }
    }

    [PunRPC]
    private IEnumerator StartOpen(Transform _itemTr)
    {
            // 상태에 따라 목표 위치 결정
            if (_doorObject == null) yield break;
        // Vector3 targetPos = _isOpen ? _openPosition : _closedPosition;

        _itemTr.transform.SetParent(attachPoint);
        _itemTr.localPosition = Vector3.zero;
        _itemTr.rotation = Quaternion.identity;

        float elapcedTime = 0f;

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
        if (!_isOpen)
        {
            Debug.Log("잠겨있습니다. 덜컹덜컹.");
            // 여기에 '덜컹'거리는 효과음이나 살짝 움직이는 연출 추가 가능
        }
    }
}