using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MoveDiration
{
    Horizontal,
    Vertical,
}

public class BaseMoveMgr : MonoBehaviour
{

    public event Action<int, GameObject> UpdateData;

    public BaseMoveItem[] Items; //所有item

    public MoveDiration Dir = MoveDiration.Horizontal; //移动方向
    public int MaxCount = 20; //总单位数
    public int MaxCountInPage = 6; //一次出现个数（显示5个，缓存1个，共6个）
    public float RetSpeed = 100; //回弹速度
    public bool Inertance = true   ; //是否使用惯性
    public float InertiaForce = 10; //惯性力大小（越大开始惯性滑动时越快）
    public float InertiaResistance = 5; //惯性力阻力（越大停的越快）

    public int CurSelected = 0; //当前选中的图片索引
    public int CurIndex = 0; //当前选中的道具索引

    public bool Moving { get; set; } //是否正在移动
    public bool Returning { get; set; } //是否正在回弹

    public  ItemInfo[] ItemInfos; //初始位置信息
    public float[] Intervals; //各个间隔长度（从左算起）
    public int midIndex; //左起第几个为中间
    protected Vector3 deltaSpeed = Vector3.zero;
    protected Vector3 inertiaDirection = Vector3.zero;
    private Queue<float> recordFrame = new Queue<float>(5);//记录最后的几帧，用于判断惯性移动方向
    public  UiMain uiMain;
    protected virtual void OnEnable()
    {
        Moving = false;
        Returning = false;
    }

    public virtual void Start()
    {
        uiMain = GameObject.FindGameObjectWithTag("UIMain").GetComponent<UiMain>();
        Items = GetComponentsInChildren<BaseMoveItem>();
        ItemInfos = new ItemInfo[Items.Length];
        for (int i = 0; i < Items.Length; i++)
        {
            ItemInfos[i] = new ItemInfo()
            {
                //Pos = new Vector3((int)Items[i].transform.position.x, (int)Items[i].transform.position.y, (int)Items[i].transform.position.z),
                Pos = Items[i].transform.position,
                Rotation = Items[i].transform.rotation,
                Scale = Items[i].transform.localScale,
            };
        }
        midIndex = (Items.Length - 2) / 2;//因为会多预留1个，所以要减2而不是1，相当于(（Items.Length - 1） - 1) / 2
        Intervals = new float[Items.Length];
        float Interval = 1f / (ItemInfos.Length - 1);
        for (int i = 0; i < Intervals.Length; i++)
        {
            Intervals[i] = Interval * i;
        }
        Intervals[Intervals.Length - 1] = 1;
        float offset2 = 0;
        float interval1 = 0;
        float overstep = 0;

        switch (Dir)
        {
            case MoveDiration.Horizontal:
                offset2 = ItemInfos[0].Pos.x;
                interval1 = ItemInfos[ItemInfos.Length - 1].Pos.x - ItemInfos[0].Pos.x;
                break;
            case MoveDiration.Vertical:
                offset2 = ItemInfos[0].Pos.y;
                interval1 = ItemInfos[ItemInfos.Length - 1].Pos.y - ItemInfos[0].Pos.y;
                break;
        }
        overstep = interval1 * 0.1f;

        for (int i = 0; i < Items.Length - 1; i++)
        {
            Items[i].Offset2 = offset2;
            Items[i].Interval1 = interval1;
            Items[i].Overstep = overstep;
            Items[i].OnDragDelta += OnMoveDelta;
            Items[i].SetMgr(this);
            Items[i].InitDate(i);
            Items[i].OriginX = interval1 * Intervals[i] + offset2;
            Items[i].UpdatePos(Vector3.zero);
            UpdatePos(Items[i]);
        }

        Items[Items.Length - 1].gameObject.SetActive(false); //最后一个不需要显示，仅用来计算距离
    }

    /// <summary>
    /// 开始移动
    /// </summary>
    public virtual void StartMove()
    {
        if (MaxCount < 2 || MaxCountInPage < 2) return;

        Moving = true;
        recordFrame.Clear();
    }

    /// <summary>
    /// 移动整个展示条
    /// </summary>
    /// <param name="delta"></param>
    public virtual void OnMoveDelta(Vector3 delta)
    {
        if (!Moving && !Returning) return;

        switch (Dir)
        {
            case MoveDiration.Horizontal:
                delta.y = 0;
                delta.z = 0;
                recordFrame.Enqueue(delta.x);
                break;
            case MoveDiration.Vertical:
                delta.x = 0;
                delta.z = 0;
                recordFrame.Enqueue(delta.y);
                break;
        }
        //inertiaDirection = delta;

        for (int i = 0; i < Items.Length - 1; i++)
        {
            Items[i].UpdatePos(delta);
            UpdatePos(Items[i]);
        }
    }

    /// <summary>
    /// 对每个Item进行记录信息插值
    /// </summary>
    /// <param name="item"></param>
    public virtual void UpdatePos(BaseMoveItem item)
    {
        int index = 0;
        for (int i = 0; i < Intervals.Length; i++)
        {
            if (item.Progress < Intervals[i])
            {
                index = i;
                break;
            }
        }
        if (index <= 0)
        {
            index = 1;
        }
       
        float t = (item.Progress - Intervals[index - 1]) / (Intervals[index] - Intervals[index - 1]);
        item.transform.position = Vector3.Lerp(ItemInfos[index - 1].Pos, ItemInfos[index].Pos, t);
        item.transform.rotation = Quaternion.Lerp(ItemInfos[index - 1].Rotation, ItemInfos[index].Rotation, t);
        item.transform.localScale = Vector3.Lerp(ItemInfos[index - 1].Scale, ItemInfos[index].Scale, t);
    }

    /// <summary>
    /// 移动结束
    /// </summary>
    public virtual void MoveOver()
    {
        Moving = false;
        if (Inertance)
        {
            StopAllCoroutines();
            StartCoroutine(MoveInertance());
        }
        else
        {
            UpdateSelected();

            StopAllCoroutines();
            StartCoroutine(ReT());
        }
    }

    /// <summary>
    /// 惯性移动
    /// </summary>
    /// <returns></returns>
    protected virtual IEnumerator MoveInertance()
    {
        Returning = true;
        Action<float> setinertiaDir = null;

        float inertiaDelta = 0;
        switch (Dir)
        {
            case MoveDiration.Horizontal:
                //inertiaDelta = MathHelper.GetDir(inertiaDirection.x);
                inertiaDirection.y = 0;
                inertiaDirection.z = 0;
                setinertiaDir += f => inertiaDirection.x = f;
                break;
            case MoveDiration.Vertical:
                //inertiaDelta = MathHelper.GetDir(inertiaDirection.y);
                inertiaDirection.x = 0;
                inertiaDirection.z = 0;
                setinertiaDir += f => inertiaDirection.y = f;
                break;
        }
        if (recordFrame.Count == 0)
        {
            inertiaDelta = 0;
        }
        else
        {
            float dirAll = 0;
            foreach (float f in recordFrame)
            {
                dirAll += f;
            }
            inertiaDelta = MathHelper.GetDir(dirAll);
        }
        inertiaDelta *= InertiaForce;

        while (true)
        {
            // Debug.Log("6666666666666666666666666666");
            inertiaDelta = Mathf.Lerp(inertiaDelta, 0, Time.deltaTime * InertiaResistance);

            if (setinertiaDir == null) yield break;
            setinertiaDir(inertiaDelta);

            OnMoveDelta(inertiaDirection);

            if (Mathf.Abs(inertiaDelta) < 1)
            {
                //Moving = false;

                UpdateSelected();

                yield return ReT();
                yield break;
            }
            yield return null;
        }
    }

    /// <summary>
    /// 更新选中单位
    /// </summary>
    protected virtual void UpdateSelected()
    {
       
        float last = float.MaxValue;
        float c = 0;
        for (int i = 0; i < MaxCountInPage; i++)
        {
            c = Mathf.Abs(Items[i].Progress - Intervals[midIndex]);
            if (c < last)
            {
                CurIndex = i;
                CurSelected = Items[i].Index;
               
                last = c;
            }
        }
        uiMain.isClick = true;
        if (UpdateData != null) UpdateData.Invoke(CurSelected, Items[CurIndex].gameObject);
    }

    /// <summary>
    /// 查找最近记录点
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    protected virtual int FindClosePoint(BaseMoveItem item)
    {
        int res = 0;
        float last = float.MaxValue;
        float c = 0;
        for (int i = 0; i < Intervals.Length; i++)
        {
            c = Mathf.Abs(item.Progress - Intervals[i]);
            if (c < last)
            {
                res = i;
                last = c;
            }
        }
        return res;
    }


    /// <summary>
    /// 回弹效果
    /// </summary>
    /// <returns></returns>
    protected virtual IEnumerator ReT()
    {
        Returning = true;

        //int dir = Items[CurIndex].Progress - Intervals[midIndex] > 0 ? -1 : 1;
        //float speed = dir * RetSpeed;
        float speed = 0;
        while (true)
        {
            speed = (Items[CurIndex].Progress - Intervals[midIndex] > 0 ? -1 : 1) * RetSpeed;
            switch (Dir)
            {
                case MoveDiration.Horizontal:
                    deltaSpeed.x = speed * Time.deltaTime;
                    break;
                case MoveDiration.Vertical:
                    deltaSpeed.y = -speed * Time.deltaTime;
                    break;
            }
            OnMoveDelta(deltaSpeed);

            float t = Mathf.Abs(Items[CurIndex].Progress - Intervals[midIndex]);
            if (t < 0.01f)
            {
                Calibration();

                Returning = false;
                yield break;
            }
            yield return null;
        }
    }

    /// <summary>
    /// 校准方法
    /// </summary>
    protected virtual void Calibration()
    {
        //默认校准
        int j = 0;
        for (int i = 0; i < MaxCountInPage; i++)
        {
            j = FindClosePoint(Items[i]);
            Items[i].transform.position = ItemInfos[j].Pos;
            Items[i].transform.rotation = ItemInfos[j].Rotation;
            Items[i].transform.localScale = ItemInfos[j].Scale;
        }
    }

    /// <summary>
    /// 改变位置
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    public virtual int ChangeItem(int offset, int index)
    {
        if (offset > 0)
        {
            index = (index + MaxCountInPage) % MaxCount;
        }
        else if (offset < 0)
        {
            index = (index - MaxCountInPage + MaxCount) % MaxCount;
        }
        return index;
    }
}