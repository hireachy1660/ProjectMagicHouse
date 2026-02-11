using UnityEngine;

public class TagChanger : MonoBehaviour
{
    [SerializeField]
    private GameStatusSO myStatus;

    private void Start()
    {
        this.gameObject.tag = myStatus.myRole;
    }
}
