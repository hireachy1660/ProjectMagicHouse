using UnityEngine;
using Oculus.Interaction;
using System.Collections.Generic;

public class HandednessInteractorFilter : MonoBehaviour, IGameObjectFilter
{
    public enum HandType { Left, Right }
    [SerializeField] private HandType allowedHandType;

    // 런타임 성능을 위해 결과를 캐싱합니다
    private readonly Dictionary<int, bool> _filterCache = new Dictionary<int, bool>();

    public bool Filter(GameObject gameObject)
    {
        int instanceId = gameObject.GetInstanceID();
        if (_filterCache.TryGetValue(instanceId, out bool result)) return result;

        IInteractor interactor = gameObject.GetComponentInParent<IInteractor>();
        if (interactor == null) return false;

        // Meta 표준 프리팹 구조 분석 (이름 기반 식별)
        bool isLeftHand = gameObject.name.Contains("Left") || gameObject.transform.root.name.Contains("Left");
        
        bool isAllowed = (allowedHandType == HandType.Left && isLeftHand) || 
                         (allowedHandType == HandType.Right && !isLeftHand);

        _filterCache[instanceId] = isAllowed;
        return isAllowed;
    }

    public void ClearCache() => _filterCache.Clear();
}