using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;

public class InventoryManager : MonoBehaviourPun
{
    [SerializeField]
    private GameObject InvenButtonPref = null;
    [SerializeField]
    private EvidenceDatabase Database = null;
    [SerializeField]
    private Transform itemParent = null;

    private Dictionary<string,InventoryButton> invenItems = new Dictionary<string,InventoryButton>();

    private void Start()
    {
        for(int i = 0; i < 9;  i++)
        {
            GameObject itemGo = Instantiate(InvenButtonPref, itemParent.position, itemParent.rotation, itemParent);
            InventoryButton invenBtn = itemGo.GetComponent<InventoryButton>();
            invenBtn.OnInvenButtonAddCallback = AddItem;
            invenBtn.OnInvenButtonUseCallback = UseItem;
        }
    }

    private void AddItem(IItem _item, InventoryButton _btnScripts)
    {
        if (_item.Type != IItem.ItemType.Door)
        {
            _btnScripts.SetButton(_item, Database.Get(_item.ItemID));

            InventoryButton btn = null;

            if (!invenItems.TryGetValue(_item.ItemID, out btn))
            {
                invenItems.Add(_item.ItemID, _btnScripts);
            }
        }
        else 
        if(invenItems.ContainsKey(_item.ItemID))
        {
            invenItems[_item.ItemID].SetPanelActive(true);
        }
        else
        {
            _btnScripts.SetButton(_item, Database.Get(_item.ItemID));
            invenItems.Add(_item.ItemID, _btnScripts);
        }

            //_btnScripts.enabled = true;
            photonView.RPC(nameof(DisSpawnItem), RpcTarget.AllBuffered, _item.PhotonViewID);

    }

    private void UseItem(IItem _item, InventoryButton _btnScrits)
    {
        InventoryButton btn = null;
        if (invenItems.TryGetValue(_item.ItemID, out btn))
        {
            photonView.RPC(nameof(SpawnItem), RpcTarget.AllBuffered, _item.PhotonViewID, _item.ItemID);
            if(_item.Type != IItem.ItemType.Door)
            {
                invenItems.Remove(_item.ItemID);
                invenItems[_item.ItemID].ClearMyInfo();
            }
            //btn.enabled = false;
        }
        
    }

    [PunRPC]
    public void SpawnItem(int _ViewID,string _DicKey)
    {
        Transform tr = PhotonView.Find(_ViewID).transform;
        tr.position = invenItems[_DicKey].transform.position;

        Rigidbody rb = tr.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
    }

    [PunRPC]
    public void DisSpawnItem(int _ViewID)
    {
        Transform tr = PhotonView.Find(_ViewID).transform;
        tr.position = itemParent.transform.position;
    }

    
    
}


