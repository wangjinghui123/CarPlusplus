using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class SubBtn : BaseMoveItem
{
    public Image MainImg;//主图
    public Text MainText;

    public SecondItemInfo Info;

    private bool isDrag = false;
   
    void Awake()
    {
      
        MainImg = GetComponent<Image>();
        MainText = GetComponentInChildren<Text>();
        OnMoveInterval += PlayScale;
    }

    public void InitDate(int index, SecondItemInfo info)
    {
        base.InitDate(index);
        //print(index+" "+info);
        MainText.text = index.ToString();
        Info = info;

        MainImg.LoadSprite(Info.SpriteName);
        MainImg.ClampImgSize();
    }

    //切换位置时渐隐
    public void PlayScale(Vector3 dir)
    {
        Transform temp = GameObject.Instantiate(gameObject, transform.parent).transform;
        temp.DOScale(Vector3.zero, 0.3f).OnComplete(() => GameObject.Destroy(temp.gameObject));
        temp.DOMove(transform.position + dir * 10, 0.28f);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
      
        base.OnPointerDown(eventData);
        isDrag = false;
    }

    public override void OnDrag(PointerEventData eventData)
    {
      
        base.OnDrag(eventData);
        isDrag = true;
      
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
       
        if (!isDrag)
        {
            //单击且无滑动
            if (!MoveMgr.Moving || MoveMgr.Returning) return;
            ((MoveBtnMgr)MoveMgr).AutoReturn(this.Index);
          
        }
        else
        {
            //滑动
            base.OnPointerUp(eventData);
           
        }
    }
}