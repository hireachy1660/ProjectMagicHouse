using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct HistorySnapshot
{
    public string locationName;
    public string achievement;
    public Sprite photo;
}

[CreateAssetMenu(fileName = "EndingData", menuName = "Detective/SO/EndingData")]
public class EndingDataSO : ScriptableObject
{
    public List<HistorySnapshot> snapshots = new List<HistorySnapshot>();
    public bool isEvidenceFound = false;

    public void AddSnapshot(string loc, string desc, Sprite img)
    {
        snapshots.Add(new HistorySnapshot { locationName = loc, achievement = desc, photo = img });
    }

    public void Clear()
    {
        snapshots.Clear();
        isEvidenceFound = false;
    }
}