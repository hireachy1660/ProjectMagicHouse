using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// IReceiver 구현. PointableCanvas로 레이가 맞으면 HandManager가 이 매니저의 OnReceiveItem/OnActivate만 호출.
/// 마지막으로 포인터가 들어온 슬롯(InventorySlot)을 기준으로 넣기/사용 동작을 수행한다.
/// </summary>
public class InventoryManagerReceiver : MonoBehaviourPun, IReceiver
{
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private EvidenceDatabase database;
    [SerializeField] private GameStatusSO gameStatus;

    [Header("SetActives")]
    [SerializeField] private GameObject invenUIs;
    //[SerializeField] private TrackedDeviceGraphicRaycaster rayTracker;
    //[SerializeField] private GraphicRaycaster raycaster;
    [SerializeField] private GameObject UIrayInteractable;

    [Header("SetPos")]
    [SerializeField] private Transform handAnchor;
    [SerializeField] private Transform headAnchor;
    [SerializeField] private Transform itemParent;
    [SerializeField] private Transform addedItemPosition;

    private Dictionary<string, InventorySlot> _invenItems = new Dictionary<string, InventorySlot>();
    private InventorySlot lastActiveSlot;
    private IItem.ItemType myItemType;

    private void Start()
    {
        for (int i = 0; i < 9; i++)
        {
            GameObject go = Instantiate(slotPrefab, itemParent.position, itemParent.rotation, itemParent);
            var slot = go.GetComponent<InventorySlot>();
            if (slot != null)
                slot.SetManager(this);
        }

        if(gameStatus.myRole == Role.Pathfinder.ToString())
        {
            myItemType = IItem.ItemType.Door;
        }
        else
        {
            myItemType = IItem.ItemType.Evidence;
        }
            TurnUI(false);
    }

    public void TurnUI(bool _isTurnOn)
    {
        if (_isTurnOn)
        {
            this.transform.position = handAnchor.TransformPoint(Vector3.forward * 1f);
            StartCoroutine(LookAtCouroutine(_isTurnOn));
        }
        else
        {
            StopCoroutine(LookAtCouroutine(_isTurnOn));
        }

        invenUIs.SetActive(_isTurnOn);
        //rayTracker.enabled = _isTurnOn;
        //raycaster.enabled = _isTurnOn;
        UIrayInteractable.SetActive(_isTurnOn);
    }

    private IEnumerator LookAtCouroutine(bool _isTurnOn)
    {
        while(_isTurnOn)
        {
            yield return null;

            transform.LookAt(headAnchor);
            transform.Rotate(0f, 180f, 0f);
        }
    }

    /// <summary> InventorySlot에서 포인터 엔터 시 호출. 이 슬롯을 마지막 활성 슬롯으로 저장. </summary>
    public void SetLastActiveSlot(InventorySlot slot)
    {
        lastActiveSlot = slot;
    }

    public void ClearLastActiveSlot(InventorySlot slot)
    {
        if (lastActiveSlot == slot)
        {
            lastActiveSlot = null; // 내가 가리키던 슬롯에서 나갈 때만 정확히 비움
        }
    }

    public void OnReceiveItem(IItem item)
    {
        if (item == null) return;
        if (lastActiveSlot == null || item.Type != myItemType) return;

        AddItem(item, lastActiveSlot);
        lastActiveSlot.SetPanelActive(true);
    }

    public void OnActivate()
    {
        if (lastActiveSlot == null) return;
        if (lastActiveSlot.CurrentItem == null) return;

        UseItem(lastActiveSlot.CurrentItem, lastActiveSlot);
        lastActiveSlot.SetPanelActive(false);
    }

    private void AddItem(IItem item, InventorySlot slot)
    {
        EvidenceData data = database != null ? database.Get(item.ItemID) : null;

        if (item.Type != IItem.ItemType.Door)
        {
            if (data != null) slot.SetButton(item, data);
            if (!_invenItems.ContainsKey(item.ItemID))
                _invenItems.Add(item.ItemID, slot);
        }
        else if (_invenItems.ContainsKey(item.ItemID))
        {
            _invenItems[item.ItemID].SetPanelActive(true);
        }
        else
        {
            if (data != null) slot.SetButton(item, data);
            _invenItems.Add(item.ItemID, slot);
        }

        photonView.RPC(nameof(DisSpawnItem), RpcTarget.AllBuffered, item.PhotonViewID);
    }

    private void UseItem(IItem item, InventorySlot slot)
    {
        if (!_invenItems.TryGetValue(item.ItemID, out InventorySlot btn)) return;

        Vector3 pos = btn.transform.position;
        photonView.RPC(nameof(SpawnItem), RpcTarget.AllBuffered, item.PhotonViewID, pos.x, pos.y, pos.z);

        if (item.Type != IItem.ItemType.Door)
        {
            btn.ClearMyInfo();
            _invenItems.Remove(item.ItemID);
        }
    }

    [PunRPC]
    public void SpawnItem(int viewID, float posX, float posY, float posZ)
    {
        PhotonView pv = PhotonView.Find(viewID);
        if (pv == null) return;
        pv.transform.position = new Vector3(posX, posY, posZ);
        pv.gameObject.SetActive(true);

        // 최적화를 위해 꺼져 있던 자식 객체들을 다시 활성화
        foreach (Transform child in pv.transform)
        {
            child.gameObject.SetActive(true);
        }

        //var rb = pv.GetComponent<Rigidbody>();
        //if (rb != null) rb.isKinematic = true;
    }

    [PunRPC]
    public void DisSpawnItem(int viewID)
    {
        PhotonView pv = PhotonView.Find(viewID);
        if (pv == null || addedItemPosition == null) return;

        // 그랩인터렉터블을 포함한 컴포넌트를 꺼서 그랩을 해제
        pv.gameObject.SetActive(false);
        pv.transform.position = addedItemPosition.position;

        // 메쉬 랜더러가 있는 자식 객체를 끔
        foreach (Transform child in pv.transform)
        {
            child.gameObject.SetActive(false);
        }

        var rb = pv.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        // 최적화 및 안전처리 후 다시 켬
        pv.gameObject.SetActive(true);
    }
}
