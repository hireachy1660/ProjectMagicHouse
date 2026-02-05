using System;
using UnityEngine;

[CreateAssetMenu(fileName = "GameProgress", menuName = "Detective/SO/GameProgress")]
public class GameProgressSO : ScriptableObject
{
    [Header("Evidence Tracking")]
    public int requiredEvidenceCount = 3; // 클리어 조건
    public int currentEvidenceCount;

    [Header("Events")]
    public Action<int> OnEvidenceAdded;
    public Action OnStageClear; // 모든 증거 수집 완료 시

    public void AddEvidence()
    {
        currentEvidenceCount++;
        OnEvidenceAdded?.Invoke(currentEvidenceCount);

        if (currentEvidenceCount >= requiredEvidenceCount)
        {
            OnStageClear?.Invoke();
        }
    }

    public void ResetProgress()
    {
        currentEvidenceCount = 0;
    }
}

