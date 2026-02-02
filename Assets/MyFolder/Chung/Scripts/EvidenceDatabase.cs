using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "EvidenceDatabase", menuName = "Detective/Database")]
public class EvidenceDatabase : ScriptableObject
{
    public List<EvidenceData> allEvidence;

    public EvidenceData Get(string id) => allEvidence.Find(x => x.id == id);
}