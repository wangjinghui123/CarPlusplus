using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public static class LoadHelper
{

    public static Sprite[] sprites;
    /// <summary>
    /// 加载图片
    /// </summary>
    public static void LoadSprite(this Image self, string spriteName)
    {
        if (sprites == null)
            sprites = Resources.LoadAll<Sprite>(@"Sprites/btn");

        for (int i = 0; i < sprites.Length; i++)
        {
          //  Debug.Log(sprites[i].name);

            if (sprites[i].name == spriteName)
            {
                self.sprite = sprites[i];
            }
        }
        //self.sprite = 
    }

    /// <summary>
    /// 加载json映射类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="assetName">json文件名（无后缀）</param>
    /// <param name="delEnd">是否删除exl转json产生的前后多余部分</param>
    /// <returns></returns>
    public static T LoadJson<T>(string assetName, bool delEnd = true)
    {
        TextAsset jsonTextAsset = null;

        jsonTextAsset = Resources.Load<TextAsset>(@"Configs/" + assetName);

        if (jsonTextAsset == null)
        {
            Debug.LogError("LoadJson Error! " + assetName);
            return default(T);
        }

        string str = jsonTextAsset.text;
        if (delEnd)
        {
            int t = str.IndexOf(':');
            str = str.Substring(t + 1, str.Length - t - 2);//删除exl转json产生的前后多余部分
        }
        T jsonObj = JsonUtility.FromJson<T>(str);
        return jsonObj;
    }

    /// <summary>
    /// 加载预制体
    /// </summary>
    /// <param name="self"></param>
    public static GameObject LoadPrefab(string assetName)
    {
        return Resources.Load<GameObject>(@"Prefabs/" + assetName);
    }
}