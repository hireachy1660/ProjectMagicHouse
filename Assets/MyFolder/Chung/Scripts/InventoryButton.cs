using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryButton : MonoBehaviour,IReceiver
{
    private EvidenceData myData;
    [SerializeField]
    private GameObject itemInfoPanel;
    [SerializeField]
    private Image itemImage;
    [SerializeField]
    private TextMeshProUGUI titleTextTMP = null;


    private IItem myIItem;

    public delegate void InvenButtonvoidIItemdelegate(IItem _item, InventoryButton _btnScript = null);
    private InvenButtonvoidIItemdelegate UseCallback;
    private InvenButtonvoidIItemdelegate AddCallback;
    public InvenButtonvoidIItemdelegate OnInvenButtonUseCallback
    { set { UseCallback = value; } } 
    public InvenButtonvoidIItemdelegate OnInvenButtonAddCallback
    { set { AddCallback = value; } }




    public void SetButton(IItem _item, EvidenceData _data)
    {
        myIItem = _item;
        myData = _data;
        if (_data == null) return;
        if (itemImage != null) itemImage.sprite = _data.icon;
        if (titleTextTMP != null) titleTextTMP.text = _data.title;
    }

    public void OnReceiveItem(IItem _item)
    {
        // 받은 아이템(_item)을 매니저에 전달해야 함. myIItem은 이 슬롯에 기존에 있던 아이템이라 넣을 때는 null
        AddCallback?.Invoke(_item, this);
        SetPanelActive(true);
    }

    public void OnActivate()
    {
        if (myIItem == null || itemInfoPanel.activeSelf) return;

        UseCallback?.Invoke(myIItem, this);
        SetPanelActive(false);
    }

    public void SetPanelActive(bool _panelActive)
    {
        itemInfoPanel.SetActive(_panelActive);
    }

    public void ClearMyInfo()
    {
        myIItem = null;
        myData = null;
        itemImage.sprite = null;
        titleTextTMP.text = string.Empty;
    }

}
