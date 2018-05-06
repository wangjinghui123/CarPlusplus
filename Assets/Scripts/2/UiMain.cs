using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiMain : MonoBehaviour
{
    public VerticalLayoutGroup VerticalGroup;

    public int CurPage = -1;//-1为没有展示出来，其他数值为展示对应页

    public bool Changing = false;
    public Text[] texts;
    public List<FirstItemInfo> FirstItemInfos;
    public Dictionary<int, List<SecondItemInfo>> SecondItemInfos = new Dictionary<int, List<SecondItemInfo>>();

    public  SwitchBtn[] switchBtns;
    public bool isClick = false;
  //  public Text   []   texts;
    private IEnumerator Start()
    {
        InitData();

        //生成一级菜单项
        if (FirstItemInfos != null && FirstItemInfos.Count != 0)
        {
            GameObject go;
            SwitchBtn sb;
            switchBtns = new SwitchBtn[FirstItemInfos.Count];
            for (int i = 0; i < switchBtns.Length; i++)
            {
                go = GameObject.Instantiate(LoadHelper.LoadPrefab("SwitchBtn"), VerticalGroup.transform,false);
                sb = go.GetComponent<SwitchBtn>();
                switchBtns[i] = sb;
                //switchBtns[i].Init(FirstItemInfos[i], SecondItemInfos[FirstItemInfos[i].Key]);
                int v1 = i;
                sb.OnClick += () => OnClickSwitchBtn(v1);
            }


            yield return new WaitForEndOfFrame();
            for (int i = 0; i < switchBtns.Length; i++)
            {
                switchBtns[i].Init(FirstItemInfos[i], SecondItemInfos[FirstItemInfos[i].Key]);
            }

            VerticalGroup.GetComponent<RectTransform>().sizeDelta = new Vector2(100, VerticalGroup.MathScrollHeight());
        }

        yield return new WaitForEndOfFrame();
        UpdateAllSwitchBtn();

        Changing = false;
    }



    /// <summary>
    /// 初始化数据
    /// </summary>
    public void InitData()
    {
        FirstItemInfoJson jsonObj = LoadHelper.LoadJson<FirstItemInfoJson>("FirstItemInfoJson");
        FirstItemInfos = jsonObj.Infos;
        foreach (var item in jsonObj.Infos)
        {
            SecondItemInfos.Add(item.Key, new List<SecondItemInfo>());
        }
        SecondItemInfoJson jsonObj2 = LoadHelper.LoadJson<SecondItemInfoJson>("SecondItemInfoJson");
        foreach (var item in jsonObj2.Infos)
        {
            if (SecondItemInfos.ContainsKey(item.ItemFrom))
            {
                SecondItemInfos[item.ItemFrom].Add(item);
            }
        }
    }

    /// <summary>
    /// 点击切换页按钮
    /// </summary>
    /// <param name="index"></param>
    public void OnClickSwitchBtn(int index)
    {
      
        if (Changing||switchBtns.Length == 0) return;
        foreach (SwitchBtn btn in switchBtns)
        {
            if (btn.MoveMgr.Moving || btn.MoveMgr.Returning) return;
        }
        StartCoroutine(ChangePage(index));
    }

    public IEnumerator ChangePage(int index)
    {
        Changing = true;
        //收拢
        if (CurPage != -1)
        {
            yield return switchBtns[CurPage].Close();
            switchBtns[CurPage].ChangState(SwitchBtnState.Normal);
            for (int i=0;i<texts .Length;i++)
            {
                texts[i].gameObject . SetActive(false );

            }
        }

        if (CurPage == index)
        {
            CurPage = -1;
        }
        else
        {
            CurPage = index;
        }

        //弹出
        if (CurPage != -1)
        {
            texts[CurPage ].gameObject.SetActive(true );
            switchBtns[CurPage].ChangState(SwitchBtnState.Selected);
            yield return switchBtns[CurPage].Show();
          
        }

        Changing = false;
    }
  
    /// <summary>
    /// 更新显示所有开关按钮
    /// </summary>
    public void UpdateAllSwitchBtn()
    {
        for (int i = 0; i < switchBtns.Length; i++)
        {
            switchBtns[i].ChangState(i == CurPage ? SwitchBtnState.Selected : SwitchBtnState.Normal);
        }
    }
}