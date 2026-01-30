using UnityEngine;
using Photon.Pun;
using System.Collections;
using Oculus.Interaction;

public class HandCamera : MonoBehaviourPun
{
    [SerializeField]
    private Transform angleTr = null;
    [SerializeField]
    private LayerMask targetLayer;
    [SerializeField]
    private Transform photoSpawnPoint = null;
    [SerializeField]
    private float animDuration = 1f;

    [Header("PicturesPrefabs")]
    [SerializeField]
    private GameObject sucessPhoto;
    [SerializeField]
    private GameObject failPhoto;


    public void OnGrabUseCamera()
    {
        Ray ray = new Ray(angleTr.position, angleTr.forward);
        RaycastHit hit;
        if(Physics.Raycast(ray, out hit, 5f,targetLayer))
        {
            photonView.RPC(nameof(SucessedCapture), RpcTarget.AllBuffered);
        }
        else
        {
            photonView.RPC(nameof(FailedCapture), RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    private void SucessedCapture()
    {
        GameObject go = Instantiate(sucessPhoto, photoSpawnPoint.localPosition, photoSpawnPoint.localRotation, photoSpawnPoint);

        StartCoroutine(PhotoAnim(go));
    }

    [PunRPC]
    private void FailedCapture()
    {
        GameObject go = Instantiate(failPhoto, photoSpawnPoint.localPosition, photoSpawnPoint.localRotation, photoSpawnPoint);

        StartCoroutine(PhotoAnim(go));
    }

    private IEnumerator PhotoAnim(GameObject _go)
    {
        _go.transform.localPosition = Vector3.zero;
        _go.transform.localRotation = Quaternion.identity;

        Vector3 startPos = Vector3.zero;
        Vector3 endPos = Vector3.up * 0.2f;

        float elapsedTime = 0f;
        while(elapsedTime <= animDuration)
        {
            _go.transform.localPosition = Vector3.Lerp(startPos, endPos, elapsedTime/animDuration);
            elapsedTime += Time.deltaTime;
            
            yield return null;
        }

        _go.transform.localPosition = endPos;
        _go.transform.SetParent(null);

        SetPhotoState(_go);
    }

    private void SetPhotoState(GameObject _go)
    {

        var goGrabSync =
        _go.GetComponentInChildren<GrabSync>();

        if (goGrabSync == null)
        {
            Debug.Log("[HandCamera] Photo dosen't have GrabSync");
            return;
        }

        goGrabSync.InitializeState(true, true, true);
    }

    
}
