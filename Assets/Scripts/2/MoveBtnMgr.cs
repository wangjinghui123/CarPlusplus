using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveBtnMgr : BaseMoveMgr
{

   

    /// <summary>
    /// 点击自动归位
    /// </summary>
    /// <param name="index"></param>
    public void AutoReturn(int index)
    {
        //print("AutoReturn");
        Moving = false;

        CurSelected = index;
        uiMain.isClick = true;
        for (int i = 0; i < Items.Length; i++)
        {
            if (Items[i].Index == CurSelected)
            {
                CurIndex = i;
                break;
            }
        }

        StopAllCoroutines();
        StartCoroutine(ReT());
      //  Debug.Log("666");
    }
   

    protected override void Calibration()
    {
        //本功能需要改动的校准方法
       // Debug.Log("滑动");
        int j = 0;
        for (int i = 0; i < MaxCountInPage; i++)
        {
            j = FindClosePoint(Items[i]);
            if (j == Intervals.Length - 1)
            {
                j = 0;
            }
            Items[i].UpdatePos(Intervals[j]);
            //UpdatePos(Items[i]);
            Items[i].transform.position = ItemInfos[j].Pos;
            Items[i].transform.rotation = ItemInfos[j].Rotation;
            Items[i].transform.localScale = ItemInfos[j].Scale;
        }
    }

   
}
