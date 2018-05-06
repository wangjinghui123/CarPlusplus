using UnityEngine;
using UnityEngine.UI;

public static class UiHelper
{
    /// <summary>
    /// 计算竖直排列控件的长度
    /// </summary>
    /// <param name="self"></param>
    /// <returns></returns>
    public static float MathScrollHeight(this VerticalLayoutGroup self)
    {
        int count = self.transform.childCount;
        if (count == 0) return 0;
        float h = self.padding.top + self.padding.bottom;
        for (int i = 0; i < count; i++)
        {
            h += self.transform.GetChild(i).GetComponent<RectTransform>().sizeDelta.y;
        }
        h += (count - 1) * self.spacing;
        return h;
    }

    /// <summary>
    /// 计算Grid控件的长度
    /// </summary>
    /// <param name="self"></param>
    /// <param name="constraintCount">每行的数量，只有constraint是Flexible才需要填入</param>
    /// <returns></returns>
    public static float MathScrollHeight(this GridLayoutGroup self, int constraintCount = 1)
    {
        //没有限制就根据输入来计算
        if (self.constraint != GridLayoutGroup.Constraint.Flexible)
        {
            constraintCount = self.constraintCount;
        }
        int count = (self.transform.childCount - 1) / constraintCount + 1;
        if (count == 0) return 0;
        float h = self.padding.top + count * self.cellSize.y + (count - 1) * self.spacing.y + self.padding.bottom;
        return h;
    }

    /// <summary>
    /// 删除所有子级物体
    /// </summary>
    /// <param name="self"></param>
    public static void DeletAllChilden(this Transform self)
    {
        if (self.childCount == 0) return;
        for (int i = 0; i < self.childCount; i++)
        {
            GameObject.Destroy(self.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// 根据现在x或y轴大小及Spirte大小进行等比例缩放,传入x或y，默认是y，修改sizeDelta
    /// </summary>
    /// <param name="axis"></param>
    public static void ClampImgSize(this Image img, string axis = "y")
    {
        Vector2 originSize = img.sprite.rect.size;
        Vector2 targetSize = Vector2.zero;
        if (axis.ToLower() == "x")
        {
            targetSize.x = img.rectTransform.sizeDelta.x;
            targetSize.y = targetSize.x * originSize.y / originSize.x;
        }
        else if (axis.ToLower() == "y")
        {
            targetSize.y = img.rectTransform.sizeDelta.y;
            targetSize.x = originSize.x * targetSize.y / originSize.y;
        }
        img.rectTransform.sizeDelta = targetSize;
    }
}