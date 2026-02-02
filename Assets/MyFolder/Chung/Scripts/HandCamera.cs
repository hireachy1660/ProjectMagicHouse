using UnityEngine;
using Photon.Pun;
using System.Collections;
using Oculus.Interaction;
using System.Collections.Generic;

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

    [Header("Sounds")]
    [SerializeField]
    private List<SoundEventSO> soundEventSOs = new List<SoundEventSO>();



    public void OnGrabUseCamera()
    {
        if (myGrabInteractable == null || myGrabInteractable.State != InteractableState.Select ) return;
        if (!photonView.IsMine) return;

        // 스폰 포인트의 자식이 있으면 리턴하는 로직 필요
        if (photoSpawnPoint.childCount > 0)
        {
            Debug.Log("[HandCamera] already have Picture");
            return;
        }

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

    private IEnumerator PhotoAnim(GameObject _go)
    {
        _go.transform.SetParent(photoSpawnPoint);

        _go.transform.localPosition = Vector3.zero;
        _go.transform.localRotation = Quaternion.identity;

        Vector3 startPos = Vector3.zero;
        Vector3 endPos = Vector3.up * 0.2f;

        PhotonTransformView myView = _go.GetComponent<PhotonTransformView>();
        //Rigidbody rb = _go.GetComponent<Rigidbody>();
        myView.enabled = false;

        float elapsedTime = 0f;
        while(elapsedTime <= animDuration)
        {
            yield return new WaitForFixedUpdate();  // 에니메이션 처리를 자연스럽게 하기위해 픽스드 없데이트 사용

            elapsedTime += Time.fixedDeltaTime; 
            float progress = elapsedTime / animDuration;

            // 2. 로컬 목표 지점 계산 (예: x축 방향으로 0.2)
            // 만약 y축이라면 Vector3.up * (0.2f * progress)
            Vector3 localOffset = new Vector3(0.2f * progress, 0, 0);
            Vector3 targetPos = photoSpawnPoint.TransformPoint(localOffset);
            
            //rb.MovePosition(targetPos);
            //rb.MoveRotation(photoSpawnPoint.rotation);

            _go.transform.localPosition = Vector3.Lerp(startPos, endPos, elapsedTime/animDuration);
            
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
