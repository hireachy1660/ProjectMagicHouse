using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using VRPortalToolkit;

public class PortalDoorController : MonoBehaviour
{
    [Header("참조")]
    public GameObject doorMesh;        // 사라질 문짝
    public Portal localPortal;         // 이 문에 붙어있는 포탈
    public XRSocketInteractor socket;  // 사진을 끼울 소켓
    public ParticleSystem burnEffect;  // 역재생 느낌을 줄 파티클 (선택사항)

    [Header("설정")]
    public float transitionSpeed = 1.5f;
    public Vector3 finalPortalScale = new Vector3(1.2f, 2.2f, 1f); // 문 크기

    private bool _isActivated = false;

    void OnEnable() { socket.selectEntered.AddListener(OnPhotoInserted); }
    void OnDisable() { socket.selectEntered.RemoveListener(OnPhotoInserted); }

    private void OnPhotoInserted(SelectEnterEventArgs args)
    {
        if (_isActivated) return;

        // 1. 소켓에 들어온 아이템이 사진인지 확인
        PhotoData photo = args.interactableObject.transform.GetComponent<PhotoData>();
        if (photo != null)
        {
            StartCoroutine(PortalOpenSequence(photo));
        }
    }

    IEnumerator PortalOpenSequence(PhotoData photo)
    {
        _isActivated = true;

        // 사진 아이템의 물리 효과를 끄고 고정
        GameObject photoObj = photo.gameObject;
        photoObj.GetComponent<Collider>().enabled = false;

        // 2. 사진이 문 크기로 커지는 연출 (불타는 역재생 이펙트 시점)
        if (burnEffect) burnEffect.Play();

        float timer = 0;
        Vector3 startScale = photoObj.transform.localScale;

        while (timer < 1.0f)
        {
            timer += Time.deltaTime * transitionSpeed;
            // 사진이 점점 커짐
            photoObj.transform.localScale = Vector3.Lerp(startScale, finalPortalScale, timer);
            // 점점 투명해지거나 밝아지는 연출을 Material에서 조절 가능
            yield return null;
        }

        // 3. 문짝 삭제 및 포탈 활성화
        doorMesh.SetActive(false); // 실제 문은 사라짐
        photoObj.SetActive(false); // 아이템 사진도 사라짐 (포탈로 교체)

        localPortal.gameObject.SetActive(true);
        localPortal.connected = photo.targetDestinationPortal; // 사진에 저장된 목적지 연결

        Debug.Log("통로가 열렸습니다!");
    }
}