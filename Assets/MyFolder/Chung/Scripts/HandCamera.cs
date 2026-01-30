using UnityEngine;
using Photon.Pun;
using System.Collections;
using Oculus.Interaction;

public class HandCamera : MonoBehaviourPun
{
    [SerializeField]
    private GrabInteractable myGrabInteractable = null;
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
        if (myGrabInteractable && myGrabInteractable.State != InteractableState.Select) return;
        if (!photonView.IsMine) return;

        Ray ray = new Ray(angleTr.position, angleTr.forward);
        bool isSuccess = Physics.Raycast(ray, out _, 5f, targetLayer);

        // 불 값에 따른 생성 객체 지정
        CaptureNetworkPhoto(isSuccess);
    }

    private void CaptureNetworkPhoto(bool isSuccess)
    {
        string prefabName = isSuccess ? sucessPhoto.name : failPhoto.name; // Resources 폴더 내 이름

        // 1. 포톤 네트워크 객체로 생성 (이러면 자동으로 모든 클라이언트에 생성되고 ID가 부여됨)
        GameObject go = PhotonNetwork.Instantiate(prefabName, photoSpawnPoint.position, photoSpawnPoint.rotation);

        // 2. 생성된 객체에 대한 애니메이션 및 상태 설정은 RPC로 모든 클라이언트에 알림
        photonView.RPC(nameof(StartPhotoProcess), RpcTarget.All, go.GetComponent<PhotonView>().ViewID);
    }

    [PunRPC]
    private void StartPhotoProcess(int viewID)
    {
        PhotonView targetView = PhotonView.Find(viewID);
        if (targetView != null)
        {
            GameObject go = targetView.gameObject;
            StartCoroutine(PhotoAnim(go));
        }
    }

    //[PunRPC]
    //private void SucessedCapture()
    //{
        
    //    // GameObject go = Instantiate(sucessPhoto, photoSpawnPoint.localPosition, photoSpawnPoint.localRotation, photoSpawnPoint);
    //    GameObject go = PhotonNetwork.Instantiate(nameof(sucessPhoto), photoSpawnPoint.localPosition, photoSpawnPoint.localRotation);
    //    go.transform.SetParent(photoSpawnPoint);

    //    StartCoroutine(PhotoAnim(go));
    //}

    //[PunRPC]
    //private void FailedCapture()
    //{
    //    GameObject go = Instantiate(failPhoto, photoSpawnPoint.position, photoSpawnPoint.rotation, photoSpawnPoint);

    //    StartCoroutine(PhotoAnim(go));
    //}

    private IEnumerator PhotoAnim(GameObject _go)
    {
        _go.transform.SetParent(photoSpawnPoint);

        _go.transform.localPosition = Vector3.zero;
        _go.transform.localRotation = Quaternion.identity;

        Vector3 startPos = Vector3.zero;
        Vector3 endPos = Vector3.up * 0.2f;

        PhotonView myView = _go.GetPhotonView();
        myView.enabled = false;

        float elapsedTime = 0f;
        while(elapsedTime <= animDuration)
        {
            _go.transform.localPosition = Vector3.Lerp(startPos, endPos, elapsedTime/animDuration);
            elapsedTime += Time.deltaTime;
            
            yield return null;
        }

        myView.enabled=true;
        _go.transform.localPosition = endPos;
        //_go.transform.SetParent(null);

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
