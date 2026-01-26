using UnityEngine;

public class SimpleLockerReceiver : MonoBehaviour, IReceiver
{
    [Header("Settings")]
    [SerializeField] private string _requiredKeyID; // 예: "Key_Locker"
    [SerializeField] private Transform _doorObject; // 움직일 문짝 (자식 오브젝트)
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

    private void Update()
    {
        if (_doorObject == null) return;

        // 상태에 따라 목표 위치 결정
        Vector3 targetPos = _isOpen ? _openPosition : _closedPosition;

        // 현재 위치에서 목표 위치로 부드럽게 이동 (Lerp)
        _doorObject.localPosition = Vector3.Lerp(_doorObject.localPosition, targetPos, Time.deltaTime * _moveSpeed);
    }

    // 인터페이스 구현: 아이템을 받았을 때
    public void OnReceiveItem(IItem item)
    {
        if (_isOpen) return; // 이미 열렸으면 패스

        if (item.ItemID == _requiredKeyID)
        {
            Debug.Log($" 사물함 잠금 해제! ({item.ItemID})");
            _isOpen = true; // 열림 상태로 전환 -> Update에서 문 이동함

            // (선택사항) 열쇠는 사용했으니 제거하거나 비활성화
            // Destroy((item as MonoBehaviour).gameObject); 
        }
        else
        {
            Debug.Log($" 맞는 열쇠가 아닙니다. (필요: {_requiredKeyID})");
        }
    }

    // 인터페이스 구현: 맨손으로 눌렀을 때
    public void OnActivate()
    {
        if (!_isOpen)
        {
            Debug.Log("잠겨있습니다. 덜컹덜컹.");
            // 여기에 '덜컹'거리는 효과음이나 살짝 움직이는 연출 추가 가능
        }
    }
}