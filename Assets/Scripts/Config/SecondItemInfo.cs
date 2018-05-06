using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �����˵�������
/// </summary>
[Serializable]
public class SecondItemInfo
{
    public int Key;
    public string ItemName;
    public int ItemFrom;
    public string SpriteName;
    public string Name;
}

/// <summary>
/// json����ӳ����
/// </summary>
[Serializable]
public class SecondItemInfoJson
{
    public int Key;
    public List<SecondItemInfo> Infos = new List<SecondItemInfo>();
}