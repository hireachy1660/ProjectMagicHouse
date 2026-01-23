using JetBrains.Annotations;
using UnityEngine;

public interface IInteractable
{
    public ItemType MyType
    { get; }
    public int MyCode
    { get;}

    public enum ItemType
    { evidence }
}
