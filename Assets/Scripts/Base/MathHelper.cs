using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathHelper
{
    /// <summary>
    /// 获取数值的方向
    /// </summary>
    /// <param name="f"></param>
    /// <returns>返回值只有正负1以及0</returns>
    public static int GetDir(float f)
    {
        if (Mathf.Abs(f) < 0.01f)
        {
            return 0;
        }
        else
        {
            if (f > 0)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        }
    }
}