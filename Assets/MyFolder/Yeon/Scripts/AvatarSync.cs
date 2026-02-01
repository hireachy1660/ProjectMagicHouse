using UnityEngine;
using Photon.Pun;

public class AvatarSync : MonoBehaviourPun
{
    [Header("Avatar parts")]
    public Transform avatarHead;
    public Transform avatarLeftHand;
    public Transform avatarRightHand;

    // 실제 vr 기기의 위치 정보를 담을 변수
    private Transform vrHead;
    private Transform vrLeftHand;
    private Transform vrRightHand;

    // CharacterSpawner에서 이 함수를 호출해서 기기를 연결해 줄거다.
    public void SetTargets(Transform hmd, Transform left, Transform right)
    {
        vrHead = hmd;
        vrLeftHand = left;
        vrRightHand = right;
    }

    private void Update()
    {
        // 내가 소환한 내 캐릭터이고, 연결된 기기 정보가 있을 때만 움직인다.
        if(photonView.IsMine && vrHead != null)
        {
            // 위치와 회전값을 그대로 복사한다.
            SyncTransform(avatarHead, vrHead);
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