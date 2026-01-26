using UnityEngine;

public class KeyObject : MonoBehaviour, IItem
{
    // 인스펙터에서 "Key_Red"라고 적어주면 됨
    [SerializeField] private string _itemID;
    [SerializeField]
    private IItem.ItemType ItemType;
    // 인터페이스 구현: 매니저가 물어보면 이 ID를 줍니다.
    public string ItemID => _itemID;
    public IItem.ItemType Type => ItemType;
}