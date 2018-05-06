using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Image))]
public class ShowImgItem : BaseMoveItem
{
    public Image MainImg;//主图
    public Text MainText;

    void Awake()
    {
        MainImg = GetComponent<Image>();
        MainText = GetComponentInChildren<Text>();
    }

    public override void InitDate(int index)
    {
        base.InitDate(index);
        MainText.text = index.ToString();
        this.LoadSprite();
    }
}

public static class ShowImgItemEx
{
    /// <summary>
    /// 加载图片
    /// </summary>
    /// <param name="self"></param>
    public static void LoadSprite(this ShowImgItem self)
    {
        //这里传入图片索引self.Index
        //根据索引加载图片
        //加载方法根据项目需求进行改动
        //self.MainImg.sprite进行赋值即可
        self.MainImg.sprite = Resources.Load<Sprite>(@"Sprites/" + self.Index);
    }
}