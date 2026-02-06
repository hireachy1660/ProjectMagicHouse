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

        itemImage.sprite = myData.icon;
        titleTextTMP.text = myData.title;

    }

    public void OnReceiveItem(IItem _item)
    {

            AddCallback?.Invoke(myIItem,this);
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
