/// <summary>
/// 플레이어가 손에 쥘 수 있는 모든 아이템(열쇠, 사진, 신발 등)의 명찰
/// </summary>
public interface IItem
{
    ItemType Type { get; }

    string ItemID { get; }
    enum ItemType
    { Other, Evidence, Door}
}