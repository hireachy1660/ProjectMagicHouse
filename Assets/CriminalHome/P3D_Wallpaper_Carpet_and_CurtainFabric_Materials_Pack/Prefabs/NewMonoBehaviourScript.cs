using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public GameObject prefab = null;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i < 4; ++i)
        {
            GameObject go = Instantiate(prefab);
            go.transform.localScale = Vector3.one * ((i + 1) * 10f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
