using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TransformOperation : MonoBehaviour
{
    public float RotaSpeed = 1;
    public float MaxDistance = 200;
    public float Resistance = 50;//阻力
    public string[] CastLayers = new[] {"Default"};
    public bool Drawing;
    public Transform CurOperateObj;

    private int touchMask = 0; //与哪些层进行碰撞
    private Vector3 rotaDelta = Vector3.zero;//旋转量
    private Vector3 lastPos = Vector3.zero;//上一帧位置
    private Vector3 posDelta = Vector3.zero;//位置偏移
    private Vector3 acceleration = Vector3.zero;//加速度，速度偏移
    private int dir = 0;//方向

    void Start()
    {
        Drawing = false;
        touchMask = LayerMask.GetMask(CastLayers);
       // Debug.Log(CastLayers.Length +"6666666666666666666666666666");
    }

    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            PointDown();
        }

        if (Input.GetMouseButtonUp(0))
        {
            PointUp();
        }

        Operation();
    }


    //按下
    private void PointDown()
    {
       
        if (Drawing) return;
        if (!EventSystem.current.IsPointerOverGameObject())
        {

            CurOperateObj = transform;
            lastPos = Input.mousePosition;
            acceleration = Vector3.zero;
            Drawing = true;
            //RaycastHit hit;
            //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
         
            //if (Physics.Raycast(ray, out hit, MaxDistance, touchMask))
            //{

              
                
            //   // Debug.Log("66666666666666666666");
            //}
        }
    }

    //弹起
    private void PointUp()
    {
        Drawing = false;
        //CurOperateObj = null;
    }

    //旋转
    private void Operation()
    {
        if (CurOperateObj == null) return;

        if (Drawing)
        {
            posDelta = lastPos - Input.mousePosition;
        }
        else
        {
            posDelta = Vector3.zero;
        }

        if (Mathf.Abs(posDelta.x) > 0.1f)
        {
            dir = posDelta.x > 0 ? 1 : -1;
            acceleration.x = dir*RotaSpeed;
            rotaDelta.y = posDelta.x * RotaSpeed * Time.deltaTime;
        }
        else
        {
            acceleration = Vector3.Lerp(acceleration, Vector3.zero, Time.deltaTime * Resistance);
            rotaDelta.y = acceleration.x * RotaSpeed * Time.deltaTime;
        }

        CurOperateObj.Rotate(rotaDelta, Space.Self);

        lastPos = Input.mousePosition;
    }
}