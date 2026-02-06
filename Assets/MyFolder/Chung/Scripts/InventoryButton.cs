using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryButton : MonoBehaviour
{
    private IItem myIItem;
    private EvidenceData myData;
    [SerializeField]
    private Image itemImage;
    [SerializeField]
    private TextMeshProUGUI titleTextTMP = null;


    public void SetButton(IItem _item, EvidenceData _data)
    {
        myIItem = _item;
        myData = _data;

        itemImage.sprite = myData.icon;
        titleTextTMP.text = myData.title;

        myIItem.Transform.position = new Vector3(0, 100f, 0);
    }

    public void OnClickInvenButton()
    {
        myIItem.Transform.position = transform.position;
        this.gameObject.SetActive(false);

    }

    public void UseItem()
    {

    }

    private void AddItem()
    {

    }

}
