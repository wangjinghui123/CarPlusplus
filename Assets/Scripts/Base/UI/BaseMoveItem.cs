using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemInfo
{
    public Vector3 Pos = Vector3.zero;
    public Quaternion Rotation = Quaternion.identity;
    public Vector3 Scale = Vector3.one;
}

public class BaseMoveItem : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public float Offset2 = 0;//第一个item的position.x偏移
    public float Interval1 = 0;//第一个item到最后一个item的间隔
    public float Overstep = 0;//左右两边超出范围
 
    /// <summary>
    /// 移动事件
    /// </summary>
    public event Action<Vector3> OnDragDelta;
    /// <summary>
    /// 超出边界，从一边转到另一边
    /// </summary>
    public event Action<Vector3> OnMoveInterval;

    public BaseMoveMgr MoveMgr;
    public int Index;//序号
    public float OriginX;//源X坐标
    public float Progress;//总体进度

    /// <summary>
    /// 初始化信息
    /// </summary>
    public virtual void InitDate(int index)
    {
        this.Index = index;
    }

    /// <summary>
    /// 设置移动管理类
    /// </summary>
    /// <param name="mgr"></param>
    public virtual void SetMgr(BaseMoveMgr mgr)
    {
        MoveMgr = mgr;
    }

    /// <summary>
    /// 更新位置信息
    /// </summary>
    /// <param name="delta">移动的偏移</param>
    public virtual void UpdatePos(Vector3 delta)
    {
        switch (MoveMgr.Dir)
        {
            case MoveDiration.Horizontal:
                OriginX += delta.x;
                break;
            case MoveDiration.Vertical:
                OriginX += delta.y;
                break;
        }

        Progress = (OriginX - Offset2) / Interval1;

        if (Progress > 1)
        {
            OriginX -= Interval1;
            Progress = (OriginX - Offset2) / Interval1;
            InitDate(MoveMgr.ChangeItem(-1, this.Index));
            if(OnMoveInterval!=null)OnMoveInterval(-new Vector3(MoveMgr.Dir == MoveDiration.Horizontal ? 1 : 0, MoveMgr.Dir == MoveDiration.Vertical ? 1 : 0, 0));
        }
        if (Progress < 0)
        {
            OriginX += Interval1;
            Progress = (OriginX - Offset2) / Interval1;
            InitDate(MoveMgr.ChangeItem(1, this.Index));
            if (OnMoveInterval != null) OnMoveInterval(new Vector3(MoveMgr.Dir == MoveDiration.Horizontal ? 1 : 0, MoveMgr.Dir == MoveDiration.Vertical ? 1 : 0, 0));
        }
    }

    /// <summary>
    /// 更新位置信息
    /// </summary>
    /// <param name="progress">进度值</param>
    public virtual void UpdatePos(float progress)
    {
        Progress = progress;

        OriginX = Progress * Interval1 + Offset2;

        UpdatePos(Vector3.zero);
       
    }

    /// <inheritdoc />
    /// <summary>
    /// 按下
    /// </summary>
    /// <param name="eventData"></param>
    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if (MoveMgr.Moving||MoveMgr.Returning) return;
        MoveMgr.StartMove();
    }

    /// <inheritdoc />
    /// <summary>
    /// 拖动接口
    /// </summary>
    /// <param name="eventData"></param>
    public virtual void OnDrag(PointerEventData eventData)
    {
        if (MoveMgr.Moving && !MoveMgr.Returning)
        {
            if (OnDragDelta != null) OnDragDelta.Invoke(eventData.delta);
        }
    }

    /// <inheritdoc />
    /// <summary>
    /// 弹起
    /// </summary>
    /// <param name="eventData"></param>
    public virtual void OnPointerUp(PointerEventData eventData)
    {
        if (!MoveMgr.Moving || MoveMgr.Returning) return;
        MoveMgr.MoveOver();
    }

   
}