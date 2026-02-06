using Photon.Pun;
using UnityEngine;

public class PortalManagerTest : MonoBehaviourPun, IReceiver
{
    [SerializeField]
    private string correctID;

    public void OnReceiveItem(IItem _item)
    {
        if(_item.ItemID == correctID)
        {
            Debug.Log("¼º°ø");
        }
    }

    public void OnActivate()
    {

    }
}
