using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// PointableCanvas + 매니저 IReceiver 구조용. IReceiver는 구현하지 않고 정보만 담는다.
/// 포인터가 들어오면 매니저에 "마지막 활성 슬롯 = 나"로 등록한다.
/// </summary>
public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject itemInfoPanel;
    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI titleTextTMP;

    private IItem _currentItem;
    private EvidenceData _currentData;

    /// <summary> 이 슬롯에 들어 있는 아이템. 없으면 null. </summary>
    public IItem CurrentItem => _currentItem;

    private void Start()
    {
        SetPanelActive(false);
    }

    /// <summary> 매니저가 슬롯 생성 시 설정. 포인터 엔터 시 이 매니저에 나를 마지막 활성 슬롯으로 알린다. </summary>
    public void SetManager(InventoryManagerReceiver _manager)
    {
        this._manager = _manager;
    }

    private InventoryManagerReceiver _manager;

    public void OnPointerEnter(PointerEventData _eventData)
    {
        _manager?.SetLastActiveSlot(this);
    }

    public void OnPointerExit(PointerEventData _eventData)
    {
        _manager.ClearLastActiveSlot(this);
    }

    public void SetButton(IItem _item, EvidenceData _data)
    {
        _currentItem = _item;
        _currentData = _data;
        if (_data == null) return;
        if (itemImage != null) itemImage.sprite = _data.icon;
        if (titleTextTMP != null) titleTextTMP.text = _data.title;
    }

    public void SetPanelActive(bool _active)
    {
        if (itemInfoPanel != null) itemInfoPanel.SetActive(_active);
    }

    public void ClearMyInfo()
    {
        _currentItem = null;
        _currentData = null;
        if (itemImage != null) itemImage.sprite = null;
        if (titleTextTMP != null) titleTextTMP.text = string.Empty;
    }
}
