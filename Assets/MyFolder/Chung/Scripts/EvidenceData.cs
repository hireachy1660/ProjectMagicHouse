using UnityEngine;


[CreateAssetMenu(fileName = "NewEvidenceData", menuName = "Detective/SO/EvidenceData")]
public class EvidenceData : ScriptableObject
{
    public string id;
    public string title;
    [TextArea] public string description;
    public Sprite icon; 
}