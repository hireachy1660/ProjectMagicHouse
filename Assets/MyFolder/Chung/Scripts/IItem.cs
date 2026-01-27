using UnityEngine;

/// <summary>
/// 플레이어가 손에 쥘 수 있는 모든 아이템(열쇠, 사진, 신발 등)의 명찰
/// </summary>
public interface IItem
{
    ItemType Type { get; }

    string ItemID { get; }

    // 퍼즐에 성공적으로 안착했을 때 호출할 메소드
    void OnPlaced();

    enum ItemType
    { Other, Evidence, Door}

    Transform Transform { get; }
}