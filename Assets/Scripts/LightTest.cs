using UnityEngine;
using System.Collections;

//定义一个Light类
public class LightTest : MonoBehaviour
{
    //定义一个时间长度
    public float duration = 1.0F;
    //定义一个红色（颜色自取）
    public Color colorRed = Color.red;
    //定义一个蓝色（颜色自取）
    public Color colorBlue = Color.blue;
    private Light light;
    private void Start()
    {
        light = this.gameObject.GetComponent<Light>();
    }
    // Update is called once per frame
    void Update()
    {
        float phi = Time.time / duration * 2 * Mathf.PI;

        //使用数学函数来实现闪光灯效果
        float amplitude = Mathf.Cos(phi) * 0.05F + 0.1F;
        light.intensity = amplitude;
        float x = Mathf.PingPong(Time.time, duration) / duration;
        light.color = Color.Lerp(colorRed, colorBlue, x);
    }
}