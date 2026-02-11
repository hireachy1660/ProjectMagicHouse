using UnityEngine;

public class PictureSpawn : MonoBehaviour
{
    [SerializeField]
    private GameProgressSO progressSO;
    [SerializeField]
    private GameObject PictureGo;

    private void Start()
    {
        progressSO.OnEvidenceAdded += ItemSpawn;
        PictureGo.SetActive(false);
    }

    public void ItemSpawn(int _curProgeress)
    {
        if(_curProgeress >= 3)
        {
            Debug.Log($"curPro : {_curProgeress}");
            PictureGo.SetActive(true);
        }
    }
}
