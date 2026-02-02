using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class SoundManager : MonoBehaviourPun
{
    [SerializeField] private List<SoundEventSO> allEvents; // 인스펙터 할당용 리스트
    private Dictionary<string, SoundEventSO> eventDict = new Dictionary<string, SoundEventSO>();
    [SerializeField] private AudioSource audioSource;

    void Awake()
    {
        // 딕셔너리 빌드 및 이벤트 구독
        foreach (var @event in allEvents)
        {
            if (!eventDict.ContainsKey(@event.soundID))
            {
                eventDict.Add(@event.soundID, @event);
                @event.OnPlayGlobalRequest += (id, vID) => {
                    // 내 로컬에서 발생한 신호를 RPC로 전파
                    photonView.RPC(nameof(PlayAudioRPC), RpcTarget.All, id, vID);
                };
            }
            @event.OnPlayLocalRequest += (clip, vID) =>
            {
                PlayAudio(clip, vID);
            };
        }
    }

    [PunRPC]
    private void PlayAudioRPC(string _id, int _viewID)
    {
        PhotonView targetView = PhotonView.Find(_viewID);
        if (targetView != null && eventDict.TryGetValue(_id, out SoundEventSO so))
        {
            // 플레이 클립엣 포인트를 사용해 재생될 위치를 지정 
            AudioSource.PlayClipAtPoint(so.GetClip(), targetView.transform.position, so.volume);
        }
    }

    private void PlayAudio(AudioClip _clip, int _viewID)
    {
        PhotonView targetView = PhotonView.Find(_viewID);
        if (targetView != null)
        {
            AudioSource.PlayClipAtPoint(_clip, targetView.transform.position);

        }
    }
}