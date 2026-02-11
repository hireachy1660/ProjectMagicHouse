using UnityEngine;

public class DoorSynch : MonoBehaviour
{
    public GameObject syncGo = null;
    private Vector3 offset;
    private Vector3 prevPos;


    private void Start()
    {
        offset = syncGo.transform.position - transform.position;
        prevPos = transform.position;
    }

    private void Update()
    {
        if (syncGo == null) return;

        Vector3 delta = transform.position - prevPos;
        syncGo.transform.position = transform.position + offset + (delta * 10f);
        syncGo.transform.rotation = transform.rotation;

        prevPos = transform.position;
    }
}
