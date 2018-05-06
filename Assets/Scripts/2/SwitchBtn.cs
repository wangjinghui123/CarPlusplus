using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public enum SwitchBtnState
{
    Normal,
    Selected,
}

[RequireComponent(typeof(Button))]
public class SwitchBtn : MonoBehaviour
{
    public static Color SelectedColor = new Color(173/255f, 173 / 255f, 173 / 255f);

    public event Action OnClick;

    public FirstItemInfo Info { get;private set; }
    public List<SecondItemInfo> SecInfos { get;private set; }

    public Transform Point;
    public MoveBtnMgr MoveMgr;
    public Vector3 PosX;

    private Image MainImg;
    private Text ShowText;
   
    private void Awake()
    {
      
        MainImg = GetComponent<Image>();
        GetComponent<Button>().onClick.AddListener(OnClickBtn);
        PosX = Point.transform.localPosition;
      
    }

    public void Init(FirstItemInfo info,List<SecondItemInfo> list)
    {
        if(ShowText==null) ShowText = GetComponentInChildren<Text>();

        this.Info = info;
        this.SecInfos = list;
        ShowText.text = Info.ItemName;

        Point.DeletAllChilden();
        int offsetY = 0;//向上偏移
        int interval = 85;//间隔
        int midIndex = 0;
        if (list.Count % 2 == 0)
        {
            //双数
            offsetY = list.Count / 2 - 1;
            offsetY *= interval;
            midIndex = (list.Count - 1) / 2;
        }
        else
        {
            //单数
            offsetY = (list.Count - 1) / 2;
            offsetY *= interval;
            midIndex = (list.Count - 1) / 2;
        }
        SubBtn go;
        for (int i = 0; i < list.Count + 1; i++)
        {
            go = GameObject.Instantiate(LoadHelper.LoadPrefab("SubBtn"), Point, false).AddComponent<SubBtn>();
            go.transform.localPosition = new Vector3(0, offsetY, 0);
            if (midIndex == i)
            {
                go.transform.localScale = Vector3.one * 1.3f;
            }
            offsetY -= interval;
            if (i < list.Count)
            {
                go.InitDate(i, list[i]);
            }
        }

        MoveMgr = Point.gameObject.AddComponent<MoveBtnMgr>();
        
		ChangeType.instence.SetMoveMgr (info.Key,MoveMgr);
        Point.gameObject.tag = "Point";
        MoveMgr.MaxCountInPage = list.Count;
        MoveMgr.MaxCount = list.Count;
        MoveMgr.Dir = MoveDiration.Vertical;
        MoveMgr.RetSpeed = 200;
        MoveMgr.Inertance = true;
        MainImg.LoadSprite(info .SpriteName );
    }

    /// <summary>
    /// 改变按钮状态
    /// </summary>
    /// <param name="state"></param>
    public void ChangState(SwitchBtnState state)
    {
        MainImg.color = state == SwitchBtnState.Normal ? Color.white : SelectedColor;
        Point.gameObject.SetActive(state == SwitchBtnState.Selected);
    }

    public IEnumerator Close()
    {
        Point.transform.localScale = Vector3.one;
        Point.transform.localPosition = PosX;
        bool complete = false;
        //Point.transform.DOScaleY(0, 0.3f).OnComplete(() => complete = true);
        Point.transform.DOScale(Vector3.zero, 0.3f).OnComplete(() => complete = true);
        Point.transform.DOLocalMove(PosX*0.5f, 0.28f);
        yield return new WaitUntil(() => complete);
    }

    public IEnumerator Show()
    {
        Point.transform.localScale = Vector3.zero;
        Point.transform.localPosition = PosX * 0.5f;
        bool complete = false;
        //Point.transform.DOScale(Vector3.one, 0.4f).OnComplete(() => complete = true);
        Point.transform.DOScale(Vector3.one, 0.4f).OnComplete(() => complete = true);
        Point.transform.DOLocalMove(PosX, 0.38f);
        yield return new WaitUntil(() => complete);
    }

    public void OnClickBtn()
    {
      
        if (OnClick != null) OnClick.Invoke();
    }
}