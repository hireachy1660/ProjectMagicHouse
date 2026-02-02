using UnityEngine;
using System;

[CreateAssetMenu(fileName = "NewSoundEvent", menuName = "Detective/SoundEvent")]
public class SoundEventSO : ScriptableObject
{
    [SerializeField] 
    public string soundID; // RPC에서 식별자로 사용할 이름
    [SerializeField] 
    private AudioClip clip;
    [SerializeField, Range(0f, 1f)] 
    public float volume = 1f;

    // CLIP 대신 ID와 ViewID를 전달
    public Action<string, int> OnPlayGlobalRequest;
    public Action<AudioClip, int> OnPlayLocalRequest;

    public AudioClip GetClip() => clip;

    public void PlayGlobal(int _viewID)
    {
        OnPlayGlobalRequest?.Invoke(soundID, _viewID);
    }

    public void PlayLocal(int _viewID)
    {
        OnPlayLocalRequest?.Invoke(clip, _viewID);
    }
}