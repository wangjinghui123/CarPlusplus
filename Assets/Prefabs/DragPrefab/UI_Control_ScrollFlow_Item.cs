using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Control_ScrollFlow_Item : MonoBehaviour
{
    private UI_Control_ScrollFlow parent;
    [HideInInspector]
    public RectTransform rect;
    public Image img;

    public float v = 0;
    private Vector3 p, s;
    /// <summary>
    /// 缩放值
    /// </summary>
    public float sv;
    // public float index = 0,index_value;
    private Color color;
    public bool isSelect = false;

    public int index;
    public ChangeType changeType;


    public void Init(UI_Control_ScrollFlow _parent)
    {
        rect = this.GetComponent<RectTransform>();
        img = this.GetComponent<Image>();
        parent = _parent;
        color = img.color;
        //  rect.localPosition -= new Vector3(100,0,0);
    }

    public void Drag(float value)
    {
        v += value;
        p = rect.localPosition;
        p.x = parent.GetPosition(v);
        rect.localPosition = p;

        color.a = parent.GetApa(v);
        img.color = color;
        sv = parent.GetScale(v);
        s.x = sv;
        s.y = sv;
        s.z = 1;
        rect.localScale = s;
    }

    public void SetScale()
    {
        if (v < 0 || v > 0.79f)
        {
            rect.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            isSelect = true;
        }
        else
        {
            rect.localScale = new Vector3(1f, 1f, 1f);
            isSelect = false;
        }
    }
    private void Update()
    {
        SetScale();


        if (isSelect)
        {

            changeType.ChangeMaterial(index);
            parent.flag = false;


        }
       

    }
}
