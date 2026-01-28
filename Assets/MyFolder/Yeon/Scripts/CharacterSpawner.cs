using UnityEngine;
using Photon.Pun;
using System.Collections;

public class CharacterSpawner : MonoBehaviour
{
    [Header("VR Rig 설정")]
    public GameObject ovrCameraRig;

    [Header("SpawnPoint")]
    [SerializeField]
    private Transform SpawnPoint1 = null;
    [SerializeField]
    private Transform SpawnPoint2 = null;


    //[Header("VR 기기 위치 (OVRCameraRig의 Anchor들)")]
    //public Transform hmdAnchor;
    //public Transform leftHandAnchor;
    //public Transform rightHandAnchor;

    private IEnumerator Start()
    {
        // 가방에 데이터가 들어올 때까지 최대 2초간 기다린다.
        float timer = 0f;
        while (!PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("MyRole"))
        {
            yield return new WaitForSeconds(0.1f);
            timer += 0.1f;

            if(timer > 5f)
            {
                Debug.LogError("[CharaterSpawner] 캐릭터의 역할 정보를 불러오는데 실패");
                yield break;
            }
        }

        if(PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("MyRole",out object roleName))
        {
            string myRole = (string)roleName;

            // 소환할 위치를 미리 변수로 만든다.(기본값 설정)
            Vector3 spawnPosition = Vector3.zero;

            // 역할 이름에 따라 위치를 다르게 정한다.
            if (myRole == "Player_A") // 역할이름이 PlayerA일 때
            {
                spawnPosition = (SpawnPoint1 == null) ? SpawnPoint1.position : new Vector3(-15f, 0f, 0f);
                //spawnPosition = new Vector3(-15f, 0f, 0f);    // 왼쪽 -15 지점
            }
            else if (myRole == "Player_B")   // 역할이름이 PlayerB일 때
            {
                spawnPosition = (SpawnPoint2 == null) ? SpawnPoint1.position : new Vector3(15f, 0f, 0f);
                //spawnPosition = new Vector3(15f, 0f, 0f);    // 오른쪽 15 지점
            }

            // 인스펙터에서 넣어준 카메라 리그를 소환 위치로 이동
            if(ovrCameraRig != null)
            {
                ovrCameraRig.transform.position = spawnPosition;
                Debug.Log($"카메라 리그를 {spawnPosition} 위치로 이동시켰습니다.");
            }

            // 그 이름을 그대로 소환한다.
            // Resources/NetworkPrefabs
            GameObject player = PhotonNetwork.Instantiate("NetworkPrefabs/" + myRole, spawnPosition, Quaternion.identity);

            //// 소환된 게 내 거라면 VR 기기 정보를 꽂아준다.
            //if(player.GetComponent<PhotonView>().IsMine)
            //{
            //    AvatarSync syncScripts = player.GetComponent<AvatarSync>();
            //    if(syncScripts != null)
            //    {
            //        syncScripts.SetTargets(hmdAnchor, leftHandAnchor, rightHandAnchor);
            //    }
            //}
            // 스포너가 넣어 주는 방식에서 싱글톤인 OVR메니저로 부터 직접 가져 오는 방식으로 변경
        }
        else
        {
            Debug.LogError("가방에서 'MyRole'을 찾을 수 없다.");
        }

    }
}