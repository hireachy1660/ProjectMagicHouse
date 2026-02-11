using UnityEngine;
using Photon.Pun;

public class AvatarSync : MonoBehaviourPun
{
    [Header("Avatar parts")]
    [SerializeField]
    private Transform avatarHead;
    [SerializeField]
    private Transform avatarLeftHand;
    [SerializeField]
    private Transform avatarRightHand;

    [Header("Ghost Visual Floating")]
    [SerializeField] private Transform ghostVisualRoot; // 부유할 비주얼 메쉬
    public float floatAmplitude = 0.2f;  // 위아래 이동 범위 (0.1m = 10cm)
    public float floatFrequency = 1.0f;  // 부유 속도
    private float startLocalY;           // 기준 높이

    // 실제 vr 기기의 위치 정보를 담을 변수
    [Header("Debug/CheckVar")]
    [SerializeField]
    private Transform vrHead;
    [SerializeField]
    private Transform vrLeftHand;
    [SerializeField]
    private Transform vrRightHand;

    // CharacterSpawner에서 이 함수를 호출해서 기기를 연결해 줄거다.
    //public void SetTargets(Transform hmd, Transform left, Transform right)
    //{
    //    vrHead = hmd;
    //    vrLeftHand = left;
    //    vrRightHand = right;
    //}

    private void Start()
    {
        if (photonView.IsMine)
        {
            FindHardWareRig();

            if (avatarHead  != null)    // 자신의 아바타는 로컬에서 메쉬가 랜더링 되지 않게 하는 처리
            {
                Renderer[] avatarMeshRenderers = this.GetComponentsInChildren<Renderer>();
                for (int i = 0; i < avatarMeshRenderers.Length; i++)
                {
                    avatarMeshRenderers[i].enabled = false;
                }
            }
        }
        else
        {
            //  내가 아닌 '남'의 캐릭터라면, 그 캐릭터에 붙은 카메라와 리스너를 즉시 파괴 
            Camera remoteCam = GetComponentInChildren<Camera>();
            if (remoteCam != null) Destroy(remoteCam);

            AudioListener remoteListener = GetComponentInChildren<AudioListener>();
            if (remoteListener != null) Destroy(remoteListener);
        }
    }

    private void FindHardWareRig()
    {
        OVRCameraRig rig; 
        if(OVRManager.instance.TryGetComponent<OVRCameraRig>(out rig))
        {
            vrHead = rig.centerEyeAnchor;
            vrLeftHand = rig.leftHandAnchor;
            vrRightHand = rig.rightHandAnchor;

        }

    }

    private void ApplyFloatingEffect()
    {
        if (ghostVisualRoot == null) return;

        // Sine 함수를 이용한 부드러운 상하 운동
        // y = sin(Time * speed) * range
        float newY = startLocalY + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;

        Vector3 localPos = ghostVisualRoot.localPosition;
        localPos.y = newY;
        ghostVisualRoot.localPosition = localPos;
    }

    private void Update()
    {
        // 부유 효과는 네트워크 동기화와 상관없이 '모든 클라이언트'에서 각자 실행
        ApplyFloatingEffect();
    }

    private void LateUpdate()
    {
        // 내가 소환한 내 캐릭터이고, 연결된 기기 정보가 있을 때만 움직인다.
        if(photonView.IsMine && vrHead != null)
        {
            // 위치와 회전값을 그대로 복사한다.
            //SyncTransform(avatarHead, vrHead);
            SyncTransform(avatarLeftHand, vrLeftHand);
            SyncTransform(avatarRightHand, vrRightHand);

            // 몸통(부모)를 머리 위치의 바닥 지점으로 이동
            // 머리의 x,z 좌표만 따라가게 해서 몸이 머리 아래에 있게 한다.
            Vector3 newRootPos = vrHead.position;
            newRootPos.y = transform.position.y; // 높이는 바닥에 고정
            transform.position = newRootPos;

            // 몸통 회전 (머리가 보는 방향을 몸도 보게 한다)
            Vector3 lookDir = vrHead.forward;
            lookDir.y = 0;   // 위아래 기울기는 무시
            transform.rotation = Quaternion.LookRotation(lookDir);
        }
    }

    private void SyncTransform(Transform avatarPart, Transform vrPart)
    {
        avatarPart.position = vrPart.position;
        avatarPart.rotation = vrPart.rotation;
    }
}