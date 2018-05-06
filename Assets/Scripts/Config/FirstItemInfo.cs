using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 一级菜单配置类
/// </summary>
[Serializable]
public class FirstItemInfo
{
    public int Key;
    public string ItemName;
    public string SpriteName;
}

/// <summary>
/// json数据映射类
/// </summary>
[Serializable]
public class FirstItemInfoJson
{
    public int Key;
    public List<FirstItemInfo> Infos = new List<FirstItemInfo>();
}