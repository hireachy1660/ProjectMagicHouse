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
        EvidenceData data = Database != null ? Database.Get(_item.ItemID) : null;

        if (_item.Type != IItem.ItemType.Door)
        {
            if (data != null) _btnScripts.SetButton(_item, data);
            if (!invenItems.ContainsKey(_item.ItemID))
                invenItems.Add(_item.ItemID, _btnScripts);
        }
        else if (invenItems.ContainsKey(_item.ItemID))
        {
            invenItems[_item.ItemID].SetPanelActive(true);
        }
        else
        {
            if (data != null) _btnScripts.SetButton(_item, data);
            invenItems.Add(_item.ItemID, _btnScripts);
        }

        photonView.RPC(nameof(DisSpawnItem), RpcTarget.AllBuffered, _item.PhotonViewID);
    }

    private void UseItem(IItem _item, InventoryButton _btnScripts)
    {
        if (!invenItems.TryGetValue(_item.ItemID, out InventoryButton btn)) return;

        Vector3 pos = btn.transform.position;
        photonView.RPC(nameof(SpawnItem), RpcTarget.AllBuffered, _item.PhotonViewID, pos.x, pos.y, pos.z);

        // Door 타입: 버튼 유지 → 몇 번이든 스폰 가능한 자판기처럼 동작
        if (_item.Type != IItem.ItemType.Door)
        {
            btn.ClearMyInfo();
            invenItems.Remove(_item.ItemID);
        }
    }

    [PunRPC]
    public void SpawnItem(int _ViewID, float _posX, float _posY, float _posZ)
    {
        PhotonView pv = PhotonView.Find(_ViewID);
        if (pv == null) return;
        pv.transform.position = new Vector3(_posX, _posY, _posZ);
        Rigidbody rb = pv.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
    }

    [PunRPC]
    public void DisSpawnItem(int _ViewID)
    {
        PhotonView pv = PhotonView.Find(_ViewID);
        if (pv == null) return;
        if (itemParent == null) return;
        pv.transform.position = itemParent.position;
    }

    
    
}


