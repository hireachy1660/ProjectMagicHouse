using UnityEngine;
using System.Collections.Generic;

public class WorldSynchronizer : MonoBehaviour
{
    public Transform realWorldParent; // 실제 집의 부모 오브젝트
    public float scaleMultiplier = 10f;

    // 이름으로 실제 물건을 빠르게 찾기 위한 사전(Dictionary)
    private Dictionary<string, Transform> realObjects = new Dictionary<string, Transform>();

    void Start()
    {
        // 실제 집 안에 있는 모든 자식 물건들을 이름 기반으로 등록
        foreach (Transform child in realWorldParent)
        {
            if (!realObjects.ContainsKey(child.name))
                realObjects.Add(child.name, child);
        }
    }

    void Update()
    {
        // 미니어처 안에 있는 모든 자식 물건들의 움직임을 실제 물건에 복사
        foreach (Transform miniChild in transform)
        {
            if (realObjects.ContainsKey(miniChild.name))
            {
                Transform realChild = realObjects[miniChild.name];

                // 1. 회전 동기화
                realChild.rotation = miniChild.rotation;

                // 2. 위치 동기화 (미니어처 중심 기준 상대 좌표 * 10)
                // 미니어처 부모의 위치를 기준으로 자식의 로컬 위치를 계산해 적용
                realChild.position = realWorldParent.position + (miniChild.localPosition * scaleMultiplier);
            }
        }
    }

    public void RefreshObjectList()
    {
        realObjects.Clear();
        foreach (Transform child in realWorldParent)
        {
            if (!realObjects.ContainsKey(child.name))
                realObjects.Add(child.name, child);
        }
    }
}