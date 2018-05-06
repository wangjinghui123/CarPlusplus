using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// һ���˵�������
/// </summary>
[Serializable]
public class FirstItemInfo
{
    public int Key;
    public string ItemName;
    public string SpriteName;
}

/// <summary>
/// json����ӳ����
/// </summary>
[Serializable]
public class FirstItemInfoJson
{
    public int Key;
    public List<FirstItemInfo> Infos = new List<FirstItemInfo>();
}