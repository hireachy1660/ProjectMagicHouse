using UnityEngine;
using TMPro;
using Meta.XR.ImmersiveDebugger.UserInterface.Generic;
using System.Collections.Generic;

public class tutorialUI : MonoBehaviour
{
    [SerializeField] Button nextBtn;
    [SerializeField] Button prevBtn;
    [SerializeField] List<GameObject> UIPageList;
    private int curIndex = 0;

    private void Awake()
    {
        for (int i = 1;  i < UIPageList.Count; i++)
        {
            UIPageList[i].SetActive(false);
        }
    }

    public void OnNextBtn()
    {
        UIPageList[curIndex].SetActive(false);
        curIndex++;
        if(curIndex > UIPageList.Count - 1)
        {
            curIndex = 0;
        }
        UIPageList[curIndex].SetActive(true);
    }

    public void OnPrevBtn()
    {
        UIPageList[curIndex].SetActive(false);
        curIndex--;
        if(curIndex < 0)
        {
            curIndex = UIPageList.Count - 1;
        }
        UIPageList[curIndex].SetActive(true);
    }
}
