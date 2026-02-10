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
    [SerializeField]
    private float photoMoveDistance = 0.17f;

    [Header("PicturesPrefabs")]
    [SerializeField]
    private GameObject emptySpacePhoto;
    [SerializeField]
    private GameObject storePhoto;
    [SerializeField]
    private GameObject failPhoto;

    [Header("Sounds")]
    [SerializeField]
    private List<SoundEventSO> soundEventSOs = new List<SoundEventSO>();
    [SerializeField]
    private SoundEventSO Chalkac;



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

        bool isEmptySpace = false;
        bool isSuccesss = false;

        RaycastHit hit;
        Ray ray = new Ray(angleTr.position, angleTr.forward);
        if (Physics.Raycast(ray, out hit, 5f, targetLayer))
        {
            isSuccesss = true;

            if(hit.transform.gameObject.CompareTag("EmptySpacePhoto"))
            {
                isEmptySpace = true;
            }
            else
            {
                isEmptySpace = false;
            }
        }
        else
        {
            isSuccesss = false;
        }

            // 불 값에 따른 생성 객체 지정
            CaptureNetworkPhoto(isSuccesss, isEmptySpace);
    }

    private void CaptureNetworkPhoto(bool isSuccess, bool _isEmptySpace)
    {
        string prefabName;

        if (!isSuccess)
        {
            prefabName = failPhoto.name;
        }
        else
        {
            prefabName = _isEmptySpace ? emptySpacePhoto.name : storePhoto.name; // Resources 폴더 내 이름
        }

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
            soundEventSOs[0].PlayLocal(photonView.ViewID);
            StartCoroutine(PhotoAnim(go));
        }
    }

    private IEnumerator PhotoAnim(GameObject _go)
    {
        _go.transform.SetParent(photoSpawnPoint);

        _go.transform.localPosition = Vector3.zero;
        _go.transform.localRotation = Quaternion.identity;


        // 이동 범위 설정 (로컬 좌표 기준)
        Vector3 startPos = Vector3.zero;
        Vector3 endPos = Vector3.forward * photoMoveDistance; // 앞으로 0.2m 이동

        PhotonTransformView myView = _go.GetComponent<PhotonTransformView>();
        Rigidbody rb = _go.GetComponent<Rigidbody>();

        // 2. 물리 및 네트워크 간섭 차단
        myView.enabled = false;
        if (rb != null)
        {
            rb.isKinematic = true; // 물리 엔진의 간섭을 막아야 수치가 변합니다.
        }

        float elapsedTime = 0f;
        while (elapsedTime < animDuration)
        {
            yield return null; // Update 주기에 맞춰 실행

            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / animDuration);

            // 3. 로컬 포지션을 직접 수정 (이게 가장 확실합니다)
            // SmoothStep을 적용하면 연출이 더 고급스러워집니다.
            float smoothedProgress = Mathf.SmoothStep(0, 1, progress);
            _go.transform.localPosition = Vector3.Lerp(startPos, endPos, smoothedProgress);
        }

        // 4. 최종 위치 확정 및 상태 복구
        _go.transform.localPosition = endPos;

        if (rb != null)
        {
            rb.isKinematic = false; // 필요 시 다시 물리 적용
        }

        myView.enabled = true;
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
